//SimAppGraph.js 1.0.0
//author: Tomas Kulhanek
//graph utilizing dygraph library to show graphs from data obtained from remote simulator
//requires dygraph.js 

//test data
var datavariables = ["Time", "CVS.heart.leftVentricle.systole.P", "CVS.systemicCirculation.systemicArtys.Pressure", "CVS.heart.leftVentricle.diastole.P", "nerves.hypothalamus.HeatCore_Temp", "nerves.respiratoryCenter.RespRate", "CVS.heart.SA_node.Rate"];
var paramnames = [];
var paramvariables = [];
var scalarvariables = [];
var barvariables = [];
var slidervariables = [];
var graphs = [];
var graphsXY = [];
var data = [[0, 121, 96, 80, 37, 15, 60], [1, 120, 95, 80, 37, 15, 60], [2, 119, 94, 80, 37, 15.1, 60], [3, 118, 93, 80, 37.1, 15.2, 65], [4, 119, 92, 79, 37.2, 15.3, 70], [5, 120, 95, 81, 37.1, 15.2, 70], [6, 122, 96, 80, 37.3, 18, 66], [7, 124, 97, 80, 37.4, 17, 64], [8, 126, 98, 80, 37.3, 15, 60], [9, 124, 97, 80, 37.2, 15, 60], [10, 122, 96, 80, 37.1, 16, 61]];
var scalardata = [];
var svgManipulation = [];
var createjsManipulation = [];

var globalstarttime = 0;
var globalstoptime = 60; //1 minute
var globalsteps = 100;
//var globalgraphs = [];
var variableset;

//adds graph into the context with specified title and extracts data from global data array
//done store graph properties to optimize the animation and interaction
function addGraph(id, title, variables, sizefactor) {
    //console.log(title);
    //console.log(variables);
    var myvariables = variables.split(',');
    var myvariablesSet = { };
    var isTime = function (c) { return /Time/.test(c); };
    //constructs set of variables within this graph
    for (i = 0; i < myvariables.length; i++) {
        myvariablesSet[myvariables[i]] = true;
        
                //if (myvariables[i]) g.setVisibility(i,);
    }
    //fix bug #173
    var myvariablesindex = {}; //hold order of the variables in a graph
    var width = $("#col1").width()*0.96;
    var height = sizefactor * width; //if sizefactor is defined then small/big graph else standard 60% of width 
    for (i = 0; i < myvariables.length; i++) {
        myvariablesindex[myvariables[i]] = datavariables.indexOf(myvariables[i]);
    }
        if (isTime(myvariables[0])) {//per time series - x-time axis is formatted, y- are selected from retrieved data
            var g;
            g = new Dygraph(
            document.getElementById(id),
            data,
            {
                title: title,
                width: width,
                height: height,
                strokeWidth: 1.5
            });
            graphs[id] = g;
            var i = 0;

            //console.log(myvariablesSet);
            //toggle visibility of variables in graph - from all data
            for (i = 1; i < datavariables.length; i++) {
                if (myvariablesSet[datavariables[i]]) {
                    //console.log("showing " + datavariables[i]);
                    g.setVisibility(i - 1, true);
                } else g.setVisibility(i - 1, false);
            }
            //globalgraphs.push(g);
        } else {//graph with custom x,y axis
            
            //converts global data from simulation to graph with only data visible to the graph
            var data2 = [];
            for (j = 0; j < data.length; j++) {
                var data3 = [];
                //push one row of data that is inside graph
                for (i in myvariablesindex) {                    
                    data3.push(data[j][myvariablesindex[i]]);
                }
                data2.push(data3);
            }

            //console.log("custom graph data");
            //console.log(data2);
            var g;
            g = new Dygraph(
            document.getElementById(id),
            data2, {
                strokeWidth: 1.5,
                title: title,
                height: height,
                width:width
                
            });
            graphsXY[id] = { graph: g, vset: myvariablesSet, vindex:myvariablesindex };
            g.setSelection(data2.length - 1);
        }
}

function addSAGraph(id,title,variables) {
    var myvariables = variables.split(',');
    var myvariablesSet = {};
    var gr2 = new Dygraph(document.getElementById(id), data, {
        //labels: [xaxis, yaxis],
        //xlabel: xaxis,
        //ylabel: yaxis,
        logscale: true,
        animatedZoom: true,
        dateWindow:  [7.0, 7.8],
        valueRange: [15, 100],
        //tomatonSelectXY: true,
        underlayCallback: function (canvas, area, g) {
            //draw area of limit borders
            console.log("gradients [0][0..2]:" + g.toDomXCoord(gradient[0][0]) + " " + g.toDomXCoord(gradient[0][1]) + " " + g.toDomXCoord(gradient[0][2]));
            var my_gradient = canvas.createRadialGradient(g.toDomXCoord(gradient[0][0]), g.toDomYCoord(gradient[0][1]), g.toDomXCoord(gradient[0][2]) - g.toDomXCoord(0),
                                g.toDomXCoord(gradient[1][0]), g.toDomYCoord(gradient[1][1]), g.toDomXCoord(gradient[1][2]) - g.toDomXCoord(0));
            my_gradient.addColorStop(0, "#ffffaa");
            my_gradient.addColorStop(.1, "#aaccaa");
            my_gradient.addColorStop(1, "#ffffff");
            canvas.fillStyle = my_gradient;
            canvas.strokeStyle = "#909090";
            for (i = 0; i < acidbaselimitborders.length; i++) {
                canvas.beginPath();
                canvas.moveTo(g.toDomXCoord(acidbaselimitborders[i][0][0]), g.toDomYCoord(acidbaselimitborders[i][0][1]));
                for (j = 1; j < acidbaselimitborders[i].length; j++) {
                    canvas.lineTo(g.toDomXCoord(acidbaselimitborders[i][j][0]), g.toDomYCoord(acidbaselimitborders[i][j][1]));
                }
                canvas.fill();
            }
            canvas.strokeStyle = "#909090";
            //draw lines of BE
            for (i = 1; i < acidbasebelines[0].length; i++) {
                canvas.beginPath();
                canvas.moveTo(g.toDomXCoord(acidbasebelines[0][0]), g.toDomYCoord(acidbasebelines[0][i]));
                for (j = 1; j < acidbasebelines.length; j++) {
                    canvas.lineTo(g.toDomXCoord(acidbasebelines[j][0]), g.toDomYCoord(acidbasebelines[j][i]));
                }
                canvas.stroke();
            } //for
            for (i = 0; i < acidbaseannotation.length; i++) {
                canvas.font = "10px Georgia";
                //console.log("x,y,text"+ g.toDomXCoord(acidbaseannotation[i][0])+" "+ g.toDomYCoord(acidbaseannotation[i][1])+" "+ acidbaseannotation[i][2]);
                cx = g.toDomXCoord(acidbaseannotation[i][0]);
                cy = g.toDomYCoord(acidbaseannotation[i][1]);
                if ((cx > 0) && (cx < 600) && (cy > 0) && (cy < 400))
                    canvas.strokeText(acidbaseannotation[i][2], cx, cy);
                //rotateText(acidbaseannotation[i][0], acidbaseannotation[i][1], acidbaseannotation[i][2],acidbase[i][3],canvas);
            }
        } //underlaycallback
    });

    gl_g1 = GraphBordersLines(title, "pH", "pCO2mmHg",
      id,
      //SABorders_url,
      //model_rest_url + samodel + "/" + sa_variables + "?callback=?",
      //SAAnnotations_url,
      true, //logaritmic scale
        [[7.4, 40, 0.03], [7.4, 40, 0.4]], [7.0, 7.8], [15, 100] //some bordesrs
    );

}

function barChartPlotter(e) {
    var ctx = e.drawingContext;
    var points = e.points;
    var y_bottom = e.dygraph.toDomYCoord(0);  // see http://dygraphs.com/jsdoc/symbols/Dygraph.html#toDomYCoord

    // This should really be based on the minimum gap
    var bar_width = 2 / 3 * (points[1].canvasx - points[0].canvasx);
    ctx.fillStyle = e.color;

    // Do the actual plotting.
    for (var i = 0; i < points.length; i++) {
        var p = points[i];
        var center_x = p.canvasx;  // center of the bar

        ctx.fillRect(center_x - bar_width / 2, p.canvasy,
        bar_width, y_bottom - p.canvasy);
        ctx.strokeRect(center_x - bar_width / 2, p.canvasy,
        bar_width, y_bottom - p.canvasy);
    }
}

function horizBarChartPlotter(e) {
    var ctx = e.drawingContext;
    var points = e.points;
    //var y_bottom = e.dygraph.toDomYCoord(0);  // see http://dygraphs.com/jsdoc/symbols/Dygraph.html#toDomYCoord
    var left_x = e.dygraph.toDomXCoord(0);
    // This should really be based on the minimum gap
    var bar_width = 2/3* (points[0].canvasy - points[1].canvasy);
    ctx.fillStyle = e.color;
    // Do the actual plotting.
    for (var i = 0; i < points.length; i++) {
        var p = points[i];
        var right_x = p.canvasx;  // center of the bar
        
        //var left_y = (p.canvasy - bar_width / 2);
        //var xwidth = right_x - left_x;
        //console.log(" " + left_x + " " + left_y+" xwidth "+ xwidth+"ywidth "+ bar_width);

        ctx.fillRect(left_x, p.canvasy-bar_width/2 ,
        right_x-left_x, bar_width);
        ctx.strokeRect(left_x, p.canvasy-bar_width/2,
        right_x - left_x, bar_width);
    }
}

function addBar2(id, title, variable, value) {
    // For horizontal bar charts, x an y values must will be "flipped"
    // from their vertical bar counterpart.
    //var myvariables = variables.split(',');
    var indexOfMyVariable = scalarvariables.indexOf(variable);
    var myData = scalardata[indexOfMyVariable];
    console.log("index " + indexOfMyVariable + " myData " + myData+" id "+id+" value "+value);
    if (!(myData)) myData = 90;
    var range = [0, value];
    if (myData > value) range = [0, myData];
    var g2;
    g2 = new Dygraph(document.getElementById(id), [[myData,1],[value,2]],
            {   plotter: horizBarChartPlotter,                
                dateWindow: range,                
                title:title,
                titleHeight: 20
            }
    );    
}


function addBar(id, title, variable, defaultvalue) {
            if (!barvariables[variable]) {
                var myBarVar = [title, id, defaultvalue];
                barvariables[variable] = myBarVar;
            }
        }
/*
function addSlider(id, title, variable, min, value, max) {
    if (!(paramvariables[variable])) {
        var mySliderVar = id;
        paramnames.push(variable);
        paramvariables[variable] = mySliderVar;
    }
}*/

//updates value of param
function updateParam(variable,value) {
    if (!(paramvariables[variable])) {
        //var myParamVar = id;
        paramnames.push(variable);
    }
    paramvariables[variable] = value;
    
}

//sets value of param, if reference is in DOM, then setting the content to the value
function setValue(id1, reference, input) {
    //console.log("printValue " + id1 + " " + reference+" "+input.value);
    var x = document.getElementById(reference);
    var y = document.getElementById(id1);
    if (x) x.innerHTML = input.value;
    updateParam(reference, input.value);
}

function addCreateJS(divid, svgid, svgelementid, svgelemtop) {
    //retrieve createjs definition from WS and add it to the div
    //selectedSimApp = id;
    $.getJSON("../graphicname/" + svgid + "?callback=?", {}, function (data) {
        createjsManipulation.push({ id: svgelementid, op: svgelemtop });
        $("#" + divid).empty();
        $("#" + divid).append('<canvas id="graphiccanvas" width="550" height="400" style=""></canvas><script>' + base64_decode(data[0].definition) + '\n init();</script>');

        var codes = $("#" + divid + "> script");
        console.log("codes");
        console.log(codes);
        for (var i = 0; i < codes.length; i++) {
            eval(codes[i].text);
        }
        updateCreatejs();
    });
}
function updateCreatejs() {
    for (var svgElement in createjsManipulation) {
        //console.log("updating createjs id " + svgManipulation[svgElement].id);
        //var shape = document.getElementById(createjsManipulation[svgElement].id);
        //set the new x radius
        
        //var op = createjsManipulation[svgElement].op.split('='); //op[0] contains attribute name
        //shape.setAttribute(op[0], evaluateOp(op[1]));
        var mFunction = createjsManipulation[svgElement].id; //gets the function name which should be called (e.g. gotoAndStop)
        var mValue = evaluateOp(createjsManipulation[svgElement].op); //gets the value - it's algebraic of variable from model and +-*/ and Math. operands
        //var exprvalue = evaluateOp([op]); //replace model variables by the number
        eval(mFunction+"("+mValue+");"); //calls the function with the calculated value
    }
}
    
function addSvg(divid,svgid,svgelementid,svgelemtop) {
    //retrieve svg definition from WS and add it to the div
    //selectedSimApp = id;
    $.getJSON("../graphicname/" + svgid + "?callback=?", {}, function (data) {
        //console.log("adding svg "+divid);               
        $("#"+divid).empty();
        $("#"+divid).append(base64_decode(data[0].definition));
        svgManipulation.push({ id: svgelementid, op: svgelemtop });
        updateSvg();
    });
}

function updateSvg() {
    for (var svgElement in svgManipulation) {
        //console.log("updating svg id " + svgManipulation[svgElement].id);
        var shape = document.getElementById(svgManipulation[svgElement].id);
        //set the new x radius
        var op = svgManipulation[svgElement].op.split('='); //op[0] contains attribute name
        shape.setAttribute(op[0], evaluateOp(op[1]));
    }
}

function evaluateOp(expr) {
    //1.replace variable from models by the values in data field
    var j;
    var variableName;
    var variableValue;
    var grow = expr.match(/[a-zA-Z][a-zA-Z\.\-\_0-9]*/g); // split('[=()+-*///^]'); //(model.lk.V)/10    
    for (j = 0; j < grow.length; j++) {
        if (grow[j] && !isKeyword(grow[j])) expr = expr.replace(grow[j], evaluateVariableValue(grow[j]));
    }
    //2.evaluate expression, the value is returned
    return eval(expr);
}

function evaluateVariableValue(vname) {
    var vindex = datavariables.indexOf(vname);
    //console.log("evaluateVariableValue:" + vname);
    //console.log(data[data.length - 1][vindex]);
    return data[data.length - 1][vindex]; //return last value
}

