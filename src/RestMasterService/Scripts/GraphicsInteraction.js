/* save simulation application via JSON to remote web service on server*/
var lastloadedid ="";
var lastloadedname ="";
var isSVGDefinition = function (c) { return /^<svg/.test(c); },
    isCreateJSDefinition = function (c) { return /^\/\/createJS/ .test(c); };

function getGraphicsName(content) {
    var idreg = /id="([^"]*)"/g;
    return idreg.exec(content)[1]; 
    //finds first "id" definition - either it is in SVG or in the first shape within SVG
}

function saveGraphic(content) {
    var SDLURL = "../graphics";
    //visualizeGraphic(content);
    var name = getGraphicsName(content);
    if (lastloadedid && lastloadedname == name)
        sdlJson = { "Id": lastloadedid, "Name": name, "Definition": base64_encode(content) };
    else
        sdlJson = { "Name": name, "Definition": base64_encode(content) };
    //console.log(sdlJson);
    $.ajax({
        type: "POST",
        url: SDLURL,
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(sdlJson),
        processData: false,
        success: function (data) {
            //console.log("saved. Response:" + data);
            initializeGraphicMenu(); //needs to be done here as the asynchronous ajax calls it here in success.
            visualizeGraphic(content);

        },
        error: function (data) {
            console.log("error:");
            console.log(data);
        }
    });
}


/* delete graphics application on the server */
function deleteGraphic(content) {
    //var tokens = lexSimAppDsl(content);
    //var parseTree = parseSimAppDsl(tokens);
    var name = getGraphicsName(content);
    if (lastloadedid && lastloadedname == name) {
        var SDLURL = "../graphics/" + lastloadedid;
        $.ajax({
            type: "DELETE",
            url: SDLURL,
            processData: false,
            success: function (data) {
                //console.log("deleted. Response:" + data);
                initializeGraphicMenu(); //needs to be done here as the asynchronous ajax calls it here in success.
            },
            error: function (data) {
                console.log("error:");
                console.log(data);
            }
        });
    }
}

var selectedSimApp = "";

function loadGraphic(id) {
    selectedSimApp = id;
    $.ajax({
        type: "GET",
        url: "../graphics/" + id,// + "?callback=?",
        data: {},
        dataType:"jsonp",
        success: function (data) {
            console.log(data);
            editor.setValue(base64_decode(data[0].definition));
            editor.refresh();
            lastloadedid = data[0].id;
            lastloadedname = data[0].name;
            visualizeGraphic(base64_decode(data[0].definition));
            //updateSimulationFrame();
            //updateDocumentation();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log("error: GET");
            console.log(errorThrown);
        }


    });
    /*$.getJSON("../graphics/" + id + "?callback=?", {}, function (data) {
        console.log(data);
        editor.setValue(base64_decode(data[0].Definition));
        editor.refresh();
        lastloadedid = data[0].Id;
        lastloadedname = data[0].Name;
        visualizeGraphic(base64_decode(data[0].Definition));
        //updateSimulationFrame();
        //updateDocumentation();
    });*/
}

SimulatorsUrl = "../graphicMetas";
function initializeGraphicMenu() {
    $.getJSON(SimulatorsUrl, null, function (data) {
        $('#models > option').remove();
        console.log("updatetestlist");
        console.log(data);
        data.forEach(function (item) {
            $('#models').append("<option value='" + item.id + "'>" + item.name + "</option>");
        });
    });
}

//var mycanvas = '';

function visualizeGraphic(svg) {

    $("#graphic").empty();
    if (isSVGDefinition(svg)) $("#graphic").append(svg);
    if (isCreateJSDefinition(svg)) {
        $("#graphic").append('<canvas id="graphiccanvas" width="550" height="400" style="background-color:#dee7e9"></canvas>');
        $("#graphic").append("<script>" + svg + "</script>");
        $("#graphic").append("<script>init();</script>");
        evaluateGraphicScripts();
    }

}

function evaluateGraphicScripts() {
    //evaluate graphs
    var codes = document.getElementById('graphic').getElementsByTagName("script");
    //console.log("codes"+codes);
    //console.log(codes);
    for (var i = 0; i < codes.length; i++) {
        eval(codes[i].text);
    }
}


