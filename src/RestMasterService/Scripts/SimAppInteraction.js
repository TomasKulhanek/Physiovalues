//SimAppInteraction.js 1.0.0
//author: Tomas Kulhanek
//interaction between user - client application in HTML and server repository
var lastloadedid;
var lastloadedname;
var iterationstoremember = 2;
var slidertimeout = 100;
var toggled = false; //automatic simulation toggled
//var baseURLprefix = "http://localhost/worker2/simulation/"; //debug value 
var baseURLprefix = "../../worker2/simulation/";
//var baseURL = "http://localhost/worker2/simulation/HumModHab.13.08.fmu"//debug value 
var baseURL ="../../worker2/simulation/HumModHab.13.08.fmu"; //TODO ask server for simulation service URL
var resultdata;

/* save simulation application via JSON to remote web service on server*/
function saveSimApp(content) {
    
    var tokens = lexSimAppDsl(content);
    var parseTree = parseSimAppDsl(tokens);
    var name = parseTree[0][1];
    var modelname = parseTree[1][1];

    var sdlJson;
    //if the lastloaded name is equal as currently saved then pass the ID so the content is updated - not duplicated on server repository
    if (lastloadedid && lastloadedname==name)
        sdlJson = { "Id": lastloadedid, "Name": name, "ReferencedModelName": modelname, "ScreenDefinition": content };
    else
        sdlJson = { "Name": name, "ReferencedModelName": modelname, "ScreenDefinition": content };
    var SDLURL = "../SimAppScreens";
    //console.log(sdlJson);
    $.ajax({
        type: "POST",
        url: SDLURL,
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(sdlJson),
        processData: false,
        success: function(data) {
            //console.log("saved. Response:" + data);
            initializeSimAppMenu(); //needs to be done here as the asynchronous ajax calls it here in success.
            
        },
        error: function(data) {
            console.log("error:");
            console.log(data);
        }
    });
}

/* delete simulation application on the server */
function deleteSimApp(content) {
    var tokens = lexSimAppDsl(content);
    var parseTree = parseSimAppDsl(tokens);
    var name = parseTree[0][1];
    if (lastloadedid && lastloadedname == name) {
        var SDLURL = "../SimAppScreens/" + lastloadedid;
        $.ajax({
            type: "DELETE",
            url: SDLURL,
            processData: false,
            success: function(data) {
                //console.log("deleted. Response:" + data);
                initializeSimAppMenu(); //needs to be done here as the asynchronous ajax calls it here in success.
            },
            error: function(data) {
                console.log("error:");
                console.log(data);
            }
        });
    }

}
/* loads sim app definition from server repository, expects name/id as input*/
var selectedSimApp = "";
function loadSimApp(name) {
    selectedSimApp = name;
    $.getJSON("../SimAppScreens/" + name + "?callback=?", {}, function (data) {
        //console.log(data);
        editor.setValue(data[0].screenDefinition);
        editor.refresh();
        lastloadedid = data[0].id;
        lastloadedname = data[0].name;
        updateSimulationFrame();
        updateDocumentation();
    });
}

var selectedModel = "";


function initializeSimAppMenu(smodel) {
    if (smodel) {
        selectedModel = smodel;
        baseURL = baseURLprefix + selectedModel;
    }
    //console.log("initialize simappmenu");
    //console.log(selectedModel);
    
    $("#appmenudiv").empty();
    var rootList = $("<ul>").attr("id","appmenu").appendTo("#appmenudiv"); //.add("<ul class=\"sf-menu\"\>");
    var elements = {}; //array of parents of an element

    function insertMenuElement(element, id) {
        //parent1.parent2.parent3.name - parent name is separated by '.' from the name, gets first parent
        var parentname = element.substr(0, element.lastIndexOf("."));

        var parent = elements[parentname];
        //if parent for the element is not yet defined and the parentname is not empty - create it recursively
        if (!parent && (parentname.length > 0)) {
            //insert parentname
            insertMenuElement(parentname);
            parent = elements[parentname];
        }
        //new list will be created on top of existing ul element or the rootlist will be updated;
        var list = parent ? parent.children("ul") : rootList;
        //if list is not yet initialized then make <ul> in html 
        if (!list.length) {
            list = $("<ul>").appendTo(parent);
        }
        //translate element to item with html content <li> ... </li>
        var item = $("<li>").appendTo(list);
        //if id is defined - for items- not parents then define onclick action 
        if (id) {
            $("<a>").attr("href", "#").attr("onclick", "loadSimApp('" + id + "')").text(element.substr(element.lastIndexOf(".") + 1)).appendTo(item);
        } else
            $("<a>").attr("href", "#").text(element.substr(element.lastIndexOf(".") + 1)).appendTo(item);
        //update array of existing elements - for usage as parents
        elements[element] = item;
    }

    //gets the list of menu items fro server repository
    $.getJSON("../SimAppScreenMetas/" + selectedModel + "?callback=?", {}, function (data) {
        $.each(data, function () {
            insertMenuElement(this.name, this.id);
        });
        $("#appmenu").menu({ "position": { my: "left top", at: "right-15 center+12"} }); //{ "position": {at: "left bottom"} }
    });
    
}

function runSimApp(buttonprops) {
    var simulationtime = parseFloat(buttonprops.value); //fix bug globalstoptime contains string 3600+60=360060 instead of integer 3600+60=3660
    iterateSimulation(simulationtime);
}

function iterateSimulation(simulationtime) {
    if (simulationtime == 0) { //restart simulation
        globalstarttime = 0;
        globalstoptime = 0;
        data = [];
        //evaluateGraphs();
    } else {
        globalstoptime += simulationtime;
        continueSimulation();
    }
}

function continuousSimApp(buttonprops) {
    toggled = buttonprops.checked;
    //console.log("check button value:" + toggled);
    if (toggled) setTimeout("iterateSimulation(1)", slidertimeout); //TODO set changeable - simulation time and real time 
}

function updateScalarData() {
    //process scalar variables, moved inside as 2 parallel request cause accessviolationexception
    if (scalarvariables.length === 0) {
        if (toggled) setTimeout("iterateSimulation(1)", slidertimeout);
        return;
    }
    var urlsuffix = "";
    var i;
    for (i = 0; i < scalarvariables.length; i++) {
        urlsuffix += "/" + scalarvariables[i];
    }
    var requestURL = baseURL + urlsuffix + "?Start=" + globalstarttime.toString() + "&Stop=" + globalstoptime.toString() + "&Steps=" + 0 + "&callback=?";
    
    $.getJSON(requestURL, function (response2) {
        scalardata = response2.Result;
        //globalstarttime = globalstoptime;
        //$("#progressbar").hide();
        //evaluateGraphs();
        var myobj;
        var myBarVar;
        for (i = 0; i < scalarvariables.length; i++) {
            //console.log("scalar variable:" + scalarvariables[i] + " value:" + scalardata[0][i]);
            myobj = document.getElementById(scalarvariables[i]);
            myBarVar = barvariables[scalarvariables[i]];
            // the scalar value is visualized as 1. bullet bar graph
            if (myBarVar) {
                //myBarVar from SimAppGraph contains at position 2 default value, at position 1 id
                var myVar = [parseFloat(myBarVar[2]), scalardata[0][i], 0];
                $("#" + myBarVar[1]).sparkline(myVar, { type: 'bullet', width: '160px', height: '20px' });                 
            } 
            // or as a 2. simple value rendered into the holding DOM element
            if (myobj) myobj.innerHTML = scalardata[0][i];
        }
        if (toggled) setTimeout("iterateSimulation(1)", slidertimeout);
    });
}

//request the data of simulation from web service
function updateData() {
    var urlsuffix = "";
    var i;
    if (datavariables.length == 0) {
        updateScalarData();
        return;
    }
    for (i = 0; i < datavariables.length; i++) {
        urlsuffix += "/" + datavariables[i];
    }
    var requestURL = baseURL + urlsuffix + "?Start=" + globalstarttime.toString() + "&Stop=" + globalstoptime.toString() + "&Steps=" + globalsteps.toString() + "&callback=?";
    //construct parameters sent to simulator
    var paramvalues = [];
    for (var pname in paramnames) {
        paramvalues.push(document.getElementById(paramvariables[pname]).valueAsNumber());  //id
    }
    var postdata = { "ParameterNames": paramnames, "ParameterValues": paramvalues };
    //console.log("posting parameter data");
    //console.log(postdata);
    //console.log(requestURL);
    $("#progressbar").show();
    $.ajax({
        type: "POST",
        contentType: 'application/json',
        dataType: "json",
        url: requestURL,
        data: JSON.stringify(postdata),
        processData: false,
        success: function (response) {
            data = response.Result;
            globalstarttime = globalstoptime;
            $("#progressbar").hide();
            evaluateGraphs();
            //window.setTimeout(updateScalarData,1000);
            updateScalarData();
        },
        error: function (response) {
            $("#progressbar").hide();
            console.log("error when updating data from server");
            console.log(response);
        }
    });
}


function updateSimulationFrame() {
    document.getElementById('simulation').innerHTML = translateToHTML(editor.getValue());
    //for debug purpose comment following, uncomment for release
    updateData(); //bug empty data cannot be drawn
    updateControlElements();
}

function reEvaluateGraphs() {
    //evaluate graphs
    //TODO if globalgraphs exists - update existing structures instead of creating new one
    
    for (var mygraph in graphs) {
        //console.log("graphs");
        //console.log(mygraph);
        graphs[mygraph].updateOptions({ 'file': data });
    };
    for (var mygraph2 in graphsXY) { //expects mygraph holds record with 'graph' and 'vset' and 'vindex'
        //transform data to data2
        var data2 = [];
        var vindex = graphsXY[mygraph2].vindex;
        for (var j = 0; j < data.length; j++) {
            var data3 = [];            
            for (var i in vindex) {
                //push one row of data that is inside graph
                data3.push(data[j][vindex[i]]);
            }
            data2.push(data3);
        }
        graphsXY[mygraph2].graph.updateOptions({ 'file': data2 });
        graphsXY[mygraph2].graph.setSelection(data2.length - 1);
    };
    //evaluate graphics SVG
    updateSvg();
    updateCreatejs();

}
function evaluateGraphs() {
    //evaluate graphs
   var codes = document.getElementById('simulation').getElementsByTagName("script");
    //console.log("codes"+codes);
    //console.log(codes);
    for (var i = 0; i < codes.length; i++) {
        eval(codes[i].text);
    }
}

//performs simulation for another times
function continueSimulation() {
    var urlsuffix = "";
    var i;
    var datavarnames = [];
    for (i = 0; i < datavariables.length; i++) {
        urlsuffix += "/" + datavariables[i];
        datavarnames.push(datavariables[i]);
    }
    //console.log("starttime " + globalstarttime + " stoptime " + globalstoptime + " steps " + globalsteps);
    //construct parameters sent to simulator
    var paramvalues = [];
    //var myparamnames = [];
    for (var pname in paramnames) {
        var myname = paramnames[pname];
        
        //console.log(document.getElementById(paramvariables[myname]));
        var myvalue = parseFloat(paramvariables[myname]);
        //console.log(myname+" "+myvalue);
        paramvalues.push(myvalue);        
        //TODO some default value - or remove 
    }
    
    //workaround for bug - JSON not using POST, when url in different domain - different port ...
    var requestURL = baseURL + urlsuffix + "?Start=" + globalstarttime + "&Stop=" + globalstoptime + "&Step=" + globalsteps + "&ParameterValues=" + paramvalues + "&ParameterNames=" + paramnames + "&callback=?";
    $("#progressbar").show();
    if (datavariables.length > 0) //there are graphs to be retrieved first
        $.getJSON(requestURL, function (response) {
            globalstarttime = globalstoptime;
            $("#progressbar").hide();
            //iterates over all retrieved data and animate it
            //console.log("Response:");
            //console.log(response.Result);
            resultdata = response.Result;
            window.intervalId= setInterval(animateGraphStep, 50);

        });
    else {
        $("#progressbar").hide();
        updateScalarData();
    }
}

//do step of animation - adds a row from the results to the data array which is rendered in a graph
function animateGraphStep() {
    if (resultdata.length > 5) {
        for (var j = 0; j < 5; j++) {
            data.push(resultdata.shift());
            if (data.length > (iterationstoremember * globalsteps)) data.shift();
        }
    
        reEvaluateGraphs();
    } else {
        clearInterval(window.intervalId);
        for (var j = 0; j < resultdata.length; j++) {
            data.push(resultdata.shift());
            if (data.length > (iterationstoremember * globalsteps)) data.shift();
        }
        reEvaluateGraphs();        
        updateScalarData();
    }

}

function updateDocumentation() {

    translateDoc('doc');//expects that in globaldoc is parsed rows of documentation
    updateDocumentationFrame("wiki", "http://en.wikipedia.org/wiki/", " from WIKIPEDIA");
    updateDocumentationFrame("wikiskripta", "http://www.wikiskripta.eu/index.php/", " from WIKISKRIPTA");
    
}

function updateDocumentationFrame(classname,urlprefix,hrefsuffix) {    
    //console.log("UpdateDocumentationFrame 2");
    var elements = document.getElementsByClassName(classname); 
    //console.log(elements);
    for (var i = 0; i < elements.length; i++) {
        var element = elements[i];
      //  console.log(elements[i].nodeName);
        if (elements[i].nodeName == "SPAN") {
            var url = "../ExternalDoc?"+classname+"=" + element.id;
            //console.log(element);
            getDocFromWS(url, element,urlprefix,hrefsuffix);
        } //if SPAN
    }  //for

}

//workaround for the XSS blocking in browsers - javascript can't get content from outside the web address - ask the web service for the content on the URL requested
function getDocFromWS(url,element,urlprefix,hrefsuffix) {
    //console.log("requesting url:" + url);
    $.get(url, function (data) {
        //cross platform xml object creation from w3schools
        //console.log("preview:" + data);

        try // Firefox, Mozilla, Opera, etc.
                    {
            parser = new DOMParser();
            xmlDoc = parser.parseFromString(data, "text/xml");
        } catch (e) {
            try //Internet Explorer
                {
                xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
                xmlDoc.async = "false";
                xmlDoc.loadXML(data);
            } catch (e) {
                alert(e.message);
                return;
            }
        }
        //gets first paragraph
        var paragraphs = xmlDoc.getElementsByTagName("p");
        var previewcontent = "";
        

        var j = 0;
        //try to get some non-zero length content from first eligible paragraph
        do {
            if (paragraphs[j])
            previewcontent = $.trim(paragraphs[j++].textContent);
            //console.log("preview " + j + ":" + previewcontent);
        } while ((previewcontent.length < 10) && (j < paragraphs.length));

        if (previewcontent.length > 300) previewcontent = previewcontent.substring(0, 300) + "...";
        element.innerHTML = "<b><a href='" + urlprefix + element.id + "'> " + element.id + " [" + hrefsuffix + "] </a></b>" + previewcontent;
        $("#accordion").accordion("refresh");
    });
}


function base64_encode(data) {
    //  discuss at: http://phpjs.org/functions/base64_encode/
    // original by: Tyler Akins (http://rumkin.com)
    // improved by: Bayron Guevara
    // improved by: Thunder.m
    // improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // improved by: Rafał Kukawski (http://kukawski.pl)
    // bugfixed by: Pellentesque Malesuada
    //   example 1: base64_encode('Kevin van Zonneveld');
    //   returns 1: 'S2V2aW4gdmFuIFpvbm5ldmVsZA=='

    var b64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
    ac = 0,
    enc = '',
    tmp_arr = [];

    if (!data) {
        return data;
    }

    do { // pack three octets into four hexets
        o1 = data.charCodeAt(i++);
        o2 = data.charCodeAt(i++);
        o3 = data.charCodeAt(i++);

        bits = o1 << 16 | o2 << 8 | o3;

        h1 = bits >> 18 & 0x3f;
        h2 = bits >> 12 & 0x3f;
        h3 = bits >> 6 & 0x3f;
        h4 = bits & 0x3f;

        // use hexets to index into b64, and append result to encoded string
        tmp_arr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4);
    } while (i < data.length);

    enc = tmp_arr.join('');

    var r = data.length % 3;

    return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);
}

function base64_decode(data) {
    //  discuss at: http://phpjs.org/functions/base64_decode/
    // original by: Tyler Akins (http://rumkin.com)
    // improved by: Thunder.m
    // improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    //    input by: Aman Gupta
    //    input by: Brett Zamir (http://brett-zamir.me)
    // bugfixed by: Onno Marsman
    // bugfixed by: Pellentesque Malesuada
    // bugfixed by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    //   example 1: base64_decode('S2V2aW4gdmFuIFpvbm5ldmVsZA==');
    //   returns 1: 'Kevin van Zonneveld'

    var b64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
    ac = 0,
    dec = '',
    tmp_arr = [];

    if (!data) {
        return data;
    }

    data += '';

    do { // unpack four hexets into three octets using index points in b64
        h1 = b64.indexOf(data.charAt(i++));
        h2 = b64.indexOf(data.charAt(i++));
        h3 = b64.indexOf(data.charAt(i++));
        h4 = b64.indexOf(data.charAt(i++));

        bits = h1 << 18 | h2 << 12 | h3 << 6 | h4;

        o1 = bits >> 16 & 0xff;
        o2 = bits >> 8 & 0xff;
        o3 = bits & 0xff;

        if (h3 == 64) {
            tmp_arr[ac++] = String.fromCharCode(o1);
        } else if (h4 == 64) {
            tmp_arr[ac++] = String.fromCharCode(o1, o2);
        } else {
            tmp_arr[ac++] = String.fromCharCode(o1, o2, o3);
        }
    } while (i < data.length);

    dec = tmp_arr.join('');

    return dec;
}  
