//SimAppParser.js 1.0.0
//author: Tomas Kulhanek
//parser of app language - DSL for screen definition of hummod browser web app
//
var graphid = 0; //global graphid generated per screen
var barid = 0; //global barid for generated bar graph
var paramid = 0;
var globalfieldset = false;
var globaldoc;
var globaldocul = false; //whether an <ul> for list of links to WIKI 

var isColumn = function (c) { return /column/.test(c); },
        isComment = function (c) { return /[ ]*#.*/ .test(c); },
        isValue = function (c) { return /value/.test(c); },
        isBar = function (c) { return /bar/.test(c); },
        isSlider = function (c) { return /slider/.test(c); },
        isRadioButton = function (c) { return /radiobutton/.test(c); },
        isLabel = function (c) { return /label/.test(c); },
        beginsDocumentation = function (c) { return /[ ]*documentation.*/.test(c); },
        isText = function (c) { return /text/ .test(c); },
        isWiki = function (c) { return /#W/ .test(c); },
        isWikiskripta = function (c) { return /#I/ .test(c); },
        isExternal = function (c) { return /#E/ .test(c); },

        isGraph = function (c) { return /^graph/.test(c); },
        isSmallGraph = function (c) { return /smallgraph/.test(c); },
        isControlTimes = function (c) { return /controltimes/.test(c); },
        isControlButtons = function (c) { return /controlbuttons/.test(c); },
        isControlDefault = function (c) { return /controldefault/.test(c); },
        isSVG = function (c) { return /svg/.test(c); },
        isCreateJS = function (c) { return /createjs/.test(c); },        
        isKeyword = function (c) {return /^Math/.test(c);};


//splits text by new row into array
function lexSimAppDsl(simappdsl) {
    var rows = simappdsl.split('\n');
    return rows;
}

//splits text in row by space into array, stops in documentation section
function parseSimAppDsl(tokens) {
    var tree = [];
    var token;
    var modtoken;
    //console.log(tokens);
    var i = 0;
    while ((i<tokens.length) && !beginsDocumentation(tokens[i])) {
        if (!isComment(tokens[i])) {
            token = tokens[i];
            //  console.log(token);
            modtoken = token.trim().split( /[ ]+/ ); //(/[ ,]/); /\b\s+/);
            //console.log(modtoken);
            //console.log(modtoken);
            tree.push(modtoken);
        }
        i++;
         
        }
    return tree;
}

function parseSimDocDsl(tokens) {
    var tree = [];
    var token;
    var modtoken;
    //console.log(tokens);
    var i = 0;
    while ((i < tokens.length) && !beginsDocumentation(tokens[i])) {
        i++;
    }
    while ((i<tokens.length)) {
      token = tokens[i];
        //  console.log(token);
        //modtoken = token.trim().split(/[ ]+/); //(/[ ,]/); /\b\s+/);
        
        //console.log(modtoken);
        //console.log(modtoken);
        tree.push(token);
        i++;
        //console.log(entry);
    }    
    globaldoc = tree;
}


function getTranslatedValue(label, reference) {
   return label + ": <i id='" + reference + "'></i><br/>\n";
}

function getTranslatedSlider(label, reference, min, value, max) {
    paramid++;
    var step = (max - min) / 20; //granularity
    return "<b>" + label + ":</b>" +
        "<input id='param" + paramid + "' type='range' min='" + min + "' max='" + max + "' value='" + value + "' step='" + step + "'onchange=\'setValue(\"param" + paramid + "\",\"" + reference + "\",this)'/>" +
            "<i id='" + reference + "'></i><br/>\n";
    //"<script type='text/javascript'>addSlider('param" + paramid + "','" + label + "','" + reference + "','" + min + "','"+value+"','"+max+"')</script><br/>";
}

// one reference
function getTranslatedBar(label, reference, defaultvalue) {
    barid++;
    return label+": <span id='bar" + barid + "' style='height:150px;width:80px'>...</span><i id='"+reference+"'></i><script type='text/javascript'>addBar('bar" + barid + "','" + label + "','" + reference + "','" + defaultvalue + "')</script><br/>\n";
}   

function getTranslatedGraph(title, variablereferences,heightfactor) {
    graphid++;
    var outrow = "";
    if (globalfieldset) outrow += "</fieldset>";
    globalfieldset = false;
    return outrow+'<div id="graph' + graphid + '"></div><script type="text/javascript">addGraph("graph' + graphid + '","' + title + '","' + variablereferences + '","'+heightfactor+ '")</script>\n';
}

function getTranslatedLabel(row) {
    row.shift();
    var outrow = "";
    if (globalfieldset) outrow += "</fieldset>";
    globalfieldset = true;
    return outrow+"<fieldset><legend>" + row.join(' ') + "</legend>\n";
}
function getTranslatedText(row) {
    row.shift();
    return "<p>" + row.join(' ') + "</p>\n";
}
function getTranslatedRadioButton(row) {
    paramid++;
    var outrow = "<form id='param"+paramid+"'><fieldset><legend>"+row[1]+"</legend>";
    var i;
    for (i = 3; i < row.length; i++) { //from 3 is label / value /label/value ...
        if (i == 3) outrow += "<input type='radio' name='param" + paramid + "' value='" + row[i + 1] + "' checked='true' onchange=\'setValue(\"param" + paramid + "\",\"" + row[2] + "\",this)'>" + row[i] + "</input>";
        else if ((i % 2) == 1) outrow += "<input type='radio' name='param" + paramid + "' value='" + row[i + 1] + "' onchange=\'setValue(\"param" + paramid + "\",\"" + row[2] + "\",this)'>" + row[i] + "</input>";
    }
    outrow += "</fieldset></form><br/>\n"; //<script type='text/javascript'>addParam('param" + paramid + "','" + row[2] + "')</script><br/>";
    return outrow;
}

function translateRow(row) {
    //console.log("translateRow");
    //console.log(row);
    if (isValue(row[0])) return getTranslatedValue(row[1], row[2]); //TODO tree[i,2] should be replaced with value getter
    if (isBar(row[0])) return getTranslatedBar(row[1], row[2], row[3]);
    if (isSlider(row[0])) return getTranslatedSlider(row[1], row[2], row[3], row[4], row[5]);
    if (isGraph(row[0])) return getTranslatedGraph(row[1], row[2], 0.6);
    if (isSmallGraph(row[0])) return getTranslatedGraph(row[1], row[2], 0.3);
    if (isLabel(row[0])) return getTranslatedLabel(row);
    if (isRadioButton(row[0])) return getTranslatedRadioButton(row);
    if (isText(row[0])) return getTranslatedText(row);
    if (isControlButtons(row[0])) setCustomTimePoints(row);
    if (isControlTimes(row[0])) setCustomTimes(row);
    if (isControlDefault(row[0])) setDefaultDuration(row);
    if (isSVG(row[0])) return getTranslatedSvg(row);
    if (isCreateJS(row[0])) return getTranslatedCreateJS(row);
    return "";
}

function getFirstColumn(tree) {
    //console.log(tree);
    var output ="";
    var i = 0;
    //console.log(tree);
    while ((i < tree.length) && !(isColumn(tree[i][0]))) {
        output += translateRow(tree[i]);
        i++;
    }
    if (globalfieldset) {//close fieldset
        output += "</fieldset>";
        globalfieldset = false;}
    return output;
}

function getSecondColumn(tree) {
    var output = "";
    var i = 0;
    while ((i < tree.length) && (!isColumn(tree[i][0]))) i++;    //skip first column
    while ((i < tree.length)) {
        output+=translateRow(tree[i]);
        i++;
    }
    if (globalfieldset) {//close fieldset
        output += "</fieldset>";
        globalfieldset = false;
    }
    return output;
}

//control menu in parser
var defaultTimePoints = ["1 Sec", "5 Sec", "20 Sec", "1 Min", "5 Min", "20 Min", "1 Hour", "6 Hour", "12 Hour", "1 Day", "7 Days", "30 Days", "60 Days", "90 Days", "Restart"];
var customTimePoints = [];
function setCustomTimePoints(row) {
    customTimePoints = row.slice(1);
}

var defaultTimeDuration = [1, 5, 20, 60, 5 * 60, 20 * 60, 60 * 60, 6 * 60 * 60, 12 * 60 * 60, 24 * 60 * 60, 7 * 24 * 60 * 60, 30 * 24 * 60 * 60, 60 * 24 * 60 * 60, 90 * 24 * 60 * 60, 0];
var customTimeDuration = [];
function setCustomTimes(row) {
    myrow = row.slice(1);
    customTimeDuration = myrow.map(parseFloat);
}

var defaultDuration = 1;
function setDefaultDuration(row) {
    defaultDuration = parseFloat(row[1]);
    globalstoptime = globalstarttime + defaultDuration;
}
function getMenuColumn(tree) {
    if (tree[0][1]) {
        var timepoints = customTimePoints.length > 0 ? customTimePoints : defaultTimePoints;
        var timeduration = customTimeDuration.length > 0 ? customTimeDuration : defaultTimeDuration;
        var htmlout = "<div id='controlbuttons'>";
        for (i = 0; i < timepoints.length; i++) {
            htmlout += "<button id='simrun' value='" + timeduration[i] + "' onclick='runSimApp(this)'>" + timepoints[i] + "</button><br/>";
        }
        //slider for remembering values
        htmlout += '<p>Iterations to remember:<i id="iterationstoremember"/></p><div id="slider"></div>';
        //play - simulation interactively
        htmlout += '<input type="checkbox" id="check" onclick="continuousSimApp(this)"/><label for="check">Automatic simulate</label>';
        //time between two simulations
        htmlout += '<p>time between simulation iteration:<i id="timeout"/></p><div id="sliderTimeout"></div>';
        htmlout += '<button id="resetData">clear graphs</button>';
        htmlout += "</div>";
        return htmlout; //   + "<script type=\"text/javascript\">$(\"#simrun\").button().refresh();</script>";
    } else return '';
}

function updateControlElements() {
    $("#controlbuttons").buttonset();
    $("#slider").slider({
        value: 2,
        min: 1,
        max: 10,
        step: 1,
        slide: function (event, ui) {
            $("#iterationstoremember").val(" "+ui.value);
            iterationstoremember = ui.value; //declared in simappinteraction
        }
    });
    $("#sliderTimeout").slider({
        value: 100,
        min: 100,
        max: 1000,
        step: 100,
        slide: function (event, ui) {
            $("#timeout").val(" " + ui.value);
            slidertimeout = ui.value; //declared in simappinteraction
        }
    });
    $("#resetData").button().click(function (event) {
        event.preventDefault();
        data = [];
    });
    $("#check").button();
}


//returns all variables from model which will be somehow visualized in a screen
function getDataVariables(tree) {
    var graphvariables = [];
    var svgvariables = [];
    scalarvariables = []; //fill global scalar variables
    var i, j;
    //parses the whole screen definition and selects variables needed for values and graphs
    for (i=0;i<tree.length;i++) {
       if (isValue(tree[i][0])|| isBar(tree[i][0])) scalarvariables.push(tree[i][2]);
       if (isGraph(tree[i][0])|| isSmallGraph(tree[i][0])) graphvariables.push(tree[i][2]);
       if (isSVG(tree[i][0])) svgvariables.push(tree[i][3]);
    }
    //console.log(graphvariables);
    var gvariables = {};
    //The graphvariables are separated by ','. 
    //Create associative array (set) so the variable name is only once in the array
    for (i = 0; i < graphvariables.length;i++ ) {
        var grow = graphvariables[i].split(',');
        //console.log("iteration "+i + " of " + graphvariables.length);
        //console.log(grow);
        for (j = 0; j < grow.length; j++) {
            if (grow[j]) gvariables[grow[j]] = true;
        }
    }
    for (i = 0; i < svgvariables.length;i++ ) {
        var grow = graphvariables[i].match(/[a-zA-Z][a-zA-Z\.\-\_]*/g);// split('[=()+-*///^]'); //height=(model.lk.V)/10
        //skip first variable name, it is name of svg element attribute
        for (j = 1; j < grow.length; j++) {
            if (grow[j] && !isKeyword(grow[j])) gvariables[grow[j]] = true;
        }
    }
    //construct of array of variable names from the set
        var mydatavariables = [];
    for (var prop in gvariables) {        
        //console.log(prop);
        mydatavariables.push(prop);        
    };
    
    return mydatavariables;    
}

function translateToHTML(simappdsl) {
    
    var tokens = lexSimAppDsl(simappdsl);
    var parseTree = parseSimAppDsl(tokens);
    //console.log("parseTree");
    //console.log(simappdsl);
    //console.log(tokens);
    //console.log(parseTree);
    
    var hout = '<div id="container3">\n<div id="container2">\n<div id="container1"><div id="col1">'; //prolog
    graphid = 0; //reset graphs
    graphs = [];
    svgManipulation = [];
    barid = 0; //reset bars
    barvariables = [];    
    paramid = 0; //resets params
    paramvariables = [];
    paramnames = [];
    //var output = evaluate(parseTree);    
    hout += getFirstColumn(parseTree);
    hout += '</div><div id="col2">';
    hout += getSecondColumn(parseTree);
    hout += '</div><div id="col3">';
    hout += getMenuColumn(parseTree);
    hout += '</div>'; //end of col3
    hout += '</div>\n</div>\n</div>'; //end of container 1 2 3
    datavariables = getDataVariables(parseTree);
    //scalarvariables = getScalarVariables(parseTree);

    //parse documentation
    parseSimDocDsl(tokens);
    //console.log(hout);
    return hout;
}

function translateWiki(row) {
    var items = row.trim().split(/[ ]+/);
    if (!globaldocul) {
        globaldocul = true;
        return '<ul><li><span class="wiki" id="' + items[1] + '"/></li>';
    }
    return '<li><span class="wiki" id="'+items[1]+'"/></li>';
}

function translateWikiskripta(row) {
    var items = row.trim().split(/[ ]+/);
    if (!globaldocul) {
        globaldocul = true;
        return '<ul><li><span class="wikiskripta" id="' + items[1] + '"/></li>';
    }
    return '<li><span class="wikiskripta" id="'+items[1]+'"/></li>';
}

function getCurrentSimulatorHref() {
    //var items = row.trim().split(/[ ]+/);
    var thisurl = document.location.origin + document.location.pathname + "#" + selectedModel + "/" + selectedSimApp;
    return '<i>Permanent link to this simulator: <a href="'+thisurl+'">'+thisurl+'</a></i>';
}
    
function translateExternal(row) {
    var items = row.trim().split(/[ ]+/);
    if (!globaldocul) {
        globaldocul = true;
        return '<ul><li><span class="external" id="' + items[1] + '"/></li>';
    }
    return '<li><span class="external" id="'+items[1]+'"/></li>';    
}


function translateDocRow(row) {
    if (beginsDocumentation(row)) return ""; //TODO recognize language
    if (isWiki(row)) return translateWiki(row);
    if (isWikiskripta(row)) return translateWikiskripta(row);
    if (isExternal(row)) return translateExternal(row);
    if (globaldocul) { //close the <ul> list of external links
        globaldocul = false;
        return "</ul>" + row;
    }
    return row;
}

function translateDoc(id) {
    var output = "";
    var i = 0;
    //console.log(tree);
    while (i < globaldoc.length) {
        output += translateDocRow(globaldoc[i]);
        i++;
    }  
    if (globaldocul) { //close the <ul> list of external links
        globaldocul = false;
        output += "</ul>";
    }
    output += getCurrentSimulatorHref();
    //console.log("documentation:" + output);
    //console.log(globaldoc);
    document.getElementById(id).innerHTML = output;
}

function getTranslatedSvg(row) {
    graphid++;
    var outrow = "";
    if (globalfieldset) outrow += "</fieldset>";
    globalfieldset = false;
    var svgid = row[1]; //expects srdce1.svg
    var svgelementid = row[2]; //expects id of element e.g. levakomora
    var svgelementop = row[3]; //expects algebraic set statement to element's attribute using model variables (all without space) e.g.: height=(model.lk.V)/10
    return outrow + '<div id="svg' + graphid + '"></div><script type="text/javascript">addSvg("svg' + graphid + '","' + svgid + '","' + svgelementid+'","' + svgelementop + '")</script>\n';
}

function getTranslatedCreateJS(row) {
    graphid++;
    var outrow = "";
    if (globalfieldset) outrow += "</fieldset>";
    globalfieldset = false;
    var svgid = row[1]; //expects srdce1.svg
    var svgelementid = row[2]; //expects id of element e.g. levakomora
    var svgelementop = row[3]; //expects algebraic set statement to element's attribute using model variables (all without space) e.g.: height=(model.lk.V)/10
    return outrow + '<div id="createjs' + graphid + '"></div><script type="text/javascript">addCreateJS("createjs' + graphid + '","' + svgid + '","' + svgelementid + '","' + svgelementop + '")</script>\n';
}