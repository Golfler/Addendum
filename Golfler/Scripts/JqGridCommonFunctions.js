/// <summary>
/// Created By: Renuka
/// Creation On: 3June, 2013
/// Description: for JqGrid common functions
/// </summary>
var openChild = '<img src="' + resUrl + 'content/images/down_grid.png" alt="open" title="open"/>';
var closeChild = '<img src="' + resUrl + 'content/images/up_grid.png" alt="close" title="close"/>';

var jsonCheckData = []; //This will holds the selected the data of jqGrid
var jqGrigIDs = [];
var jqGridNuIDs = [];
var jqGridCPIDs = [];
var jqGridCAIDs = [];
var jqGridCCIDs = [];
var jqGridCKIDs = [];
var jqGridSUPIDs = [];
//var divPaging = '<div class="pagenation">' +
//    '<div class="left_txt">@rowmsg</div>' +
//	'<div class="right_pagenation">' +
//	'<div class="next_previos">' +
//	'@paging' +
//	'</div>' +
//	'<div class="jump_page">' +
//	'@selectpage' +
//	'</div>' +
//	'</div>' +
//	'<div class="clr"></div></div>';
var pagelist = "";

var currentListID = '';
var currentPagerID = '';

var notadminpanel = document.URL.toLowerCase().indexOf('/developer/') != -1
    || document.URL.toLowerCase().indexOf('/advertiser/') != -1 || document.URL.toLowerCase().indexOf('/store/') != -1;

function bindJqGrid(gridid, pagerid, url,
columns, model, sorton, sortdir, postData,
title, datatype, mtype, loadfunc, editOn, multiselect,
allowGrouping, groupModel, paging) {

    var defaultPageSize = 10;
    currentListID = gridid;
    currentPagerID = pagerid;
    paging = true;
    if (paging) {
        paging = [30, 60, 120];
        defaultPageSize = 30;
    }
    else
        paging = [10, 20, 30];


    if ($('#ReadFlag').val() == "True") // || notadminpanel

    $("#" + gridid).jqGrid({
        url: url,
        datatype: datatype,
        mtype: mtype,
        colNames: columns,
        colModel: model,
        pager: '#' + ((pagerid == '') ? ('p' + gridid) : pagerid),
        rowNum: (pagerid == '') ? 1000 : defaultPageSize,
        rowList: paging,
        sortname: sorton,
        sortorder: sortdir,
        viewrecords: true,
        gridview: true,
        caption: title,
        multiselect: multiselect,
        multiboxonly: false,
        postData: postData,
        jsonReader: { repeatitems: false },
        height: '100%',
        autowidth: true,
        hoverrows: false,
        grouping: allowGrouping,
        groupingView: groupModel,
        loadComplete:
            function (data) {
                //   
                //even odd row class
                $('#' + gridid + ' tbody tr.ui-row-ltr:odd').addClass('even-row');
                $('#' + gridid + ' tbody tr.ui-row-ltr:even').addClass('odd-row');

                var divPaging = '<div class="pagenation_' + gridid + '">' +
                                '<div class="left_txt">@rowmsg</div>' +
                                '<div class="right_pagenation">' +
                                '<div class="next_previos">' +
                                '@paging' +
                                '</div>' +
                                '<div class="jump_page">' +
                                '@selectpage' +
                                '</div>' +
                                '</div>' +
                                '<div class="clr"></div></div>';

                //var pageDiv = $('div.pagenation');
                //pageDiv.removeClass('pagenation');
                //pageDiv.addClass('pagenation_' + gridid);

                for (var i = 0; i < model.length; i++) {
                    if (model[i].sortable != false) {
                        $('#' + gridid + '_' + model[i].name + " div").css('cursor', 'pointer');
                    }
                }

                if (data.rows.length > 0) {
                    var x = 0;
                    for (var i = 0; i < data.rows.length; i++) {
                        var id = data.rows[i].ID;
                        //var id = data.rows[i].ID == undefined ? data.rows[i].OrgId : data.rows[i].ID; //modified
                        //debugger;
                        var celValue = data.rows[i].DoActive;
                        var grpValue = data.rows[i].Grouptype;
                        var celReadValue = data.rows[i].IsRead;
                        if (celReadValue) {
                            $("#" + gridid + " tr td[title='" + id + "']").parent().addClass("highlighted");
                        }

                        var type = data.rows[i].Type;
                        ///Added by sonika grdvalue=="A" condition to enable just those link which are db users
                        ///17 dec 2015

                        var row = $("input[id=jqg_" + gridid + "_" + $("#" + gridid + " tr td[title='" + id + "']").parent().attr("id") + "]");
                        
                        if (grpValue != "" && grpValue != undefined && grpValue != "undefined") {
                            if (grpValue == "Active Directory") {
                                row.attr("disabled", "true");
                            }
                            if (grpValue == "Database" && celValue == false) {
                                row.attr("disabled", "false");
                            }
                        }
                        else {
                            if (celValue == "false") {
                                row.attr("disabled", "true");
                            }
                        }

                        //  var groupusertype = (grouptype != undefined) ? grouptype : true;
                        //        if (groupusertype)
                        //            groupusertype = row["grouptype"];
                        //             if (groupusertype == "D" ) {// || doactive == false || doactive == "false") {
                        //            if (cellvalue == "true" || cellvalue == true)
                        //                return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to deactivate' src='/Content/images/nw_active.png'/></a>";
                        //            else
                        //                return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to activate' src='/Content/images/nw_inactive.png'/></a>";
                        //   

                       
                        if (($("#hdnId").size() > 0 && id == $("#hdnId").val()) || type == false) {
                            //$("#" + gridid + " tr td[title=" + id + "]").parent().attr("disabled", "disabled");
                            row.attr("disabled", "disabled");
                        }

                        if (data.rows[i].HasChild != null && data.rows[i].HasChild.toString().toLowerCase() == "true") {
                            var voucherid = 0;
                            if (data.rows[i].VoucherId != null) {
                                voucherid = data.rows[i].VoucherId;
                            }

                            $('#' + gridid + ' .jqgrow').eq(i + x).find('td[aria-describedby="' + gridid + '_Name"]').append('<br/><a href="javascript:BindInnerGrid(' + id + ',' + voucherid + ')" id="a' + id + '">' + openChild + '</a>');

                            var mainRow = $('#' + gridid + ' .jqgrow').eq(i + x).find('td[aria-describedby="' + gridid + '_Name"]').parent();

                            $(mainRow).after('<tr id="tr' + id + '" class="tdSubOrg ' + $(mainRow).attr('class') + '">' +
                                '<td colspan="9" style="padding: 5px;">' +
                                '<div id="div' + id + '" style="width: 99%;">' + '<table class="sublist" id="sublist' + id + '"></table><div id="psublist' + id + '"></div></div></td></tr>');

                            $('#sublist' + id).fluidGrid({ example: 'td' + id, offset: -10 });
                            x++;
                        }

                        if (gridid.indexOf('sublist') != -1)
                            $('#gview_' + gridid).parent().css('border-color', 'red').css('margin', '2px');
                    }
                }
                //Open word-wrap
                $('td[role="gridcell"]').css("word-wrap", "break-word");

                //Empty Cell replaces with '-'
                $('tr[role="row"] td[title=""]').filter(function () {
                    return $(this).html() == "&nbsp;";
                }).html('-');

                //color of text Red for 'Pending'
                $('tr[role="row"] td').filter(function () {
                    return $(this).html() == "Pending";
                }).css('color', 'red');

                //Larger description text is trimed
                $('tr[role="row"] td[aria-describedby="' + gridid + '_Description"]').each(function () {
                    if ($(this).html().length > 100) {
                        var txt = $(this).html();
                        $(this).html(txt.substring(0, 97) + "...");
                    }
                });

                //Remove resize of column
                $('span.ui-jqgrid-resize').remove();
                $('#gbox_' + gridid).css('width', '100%').css('overflow-y', 'hidden'); //.parent().css('overflow-x', 'auto');
                $('#gbox_' + gridid + ' div').css('width', '99.9%');
                $('#gbox_' + gridid + ' table').css('width', '100%');
                $('#' + gridid).parent().parent().css('display', 'none');

                var norecord = "";
                var pagebar = "";
                if (pagerid != '') {
                    //Number JqGrid Paging Start                                    
                    var pagecnt = $('#' + pagerid + ' #sp_1_' + pagerid).html();
                    var previouscnt = $('#' + pagerid + ' .jqpagebtn:last').text();
                    //if (pagecnt != previouscnt) {
                    pagelist = "";
                    var inptPage = $('#' + pagerid + ' td[dir="ltr"]:first').find('input');

                    pagelist += '<li class="space10-l-r"><a href="javascript:" id="jqpagebtn_Previous">Previous</a></li>';
                    //$('.jqpagebtn').remove();
                    for (var i = 1; i <= pagecnt; i++) {
                        pagelist += '<li><a href="javascript:" onclick="SetGridPage(' + i + ',' + pagerid + ')" id="jqpagebtn_' + i + '">' + i + '</a></li>';
                        $('#' + pagerid + ' td[dir="ltr"]:first').append("<button onclick='SetGridPage(" + i + "," + pagerid + ")' class='jqpagebtn' id='jqpagebtn_" + i + "' currentPage=" + i + ">" + i + "</button>");
                    }
                    pagelist += '<li class="space10-l-r"><a href="javascript:" id="jqpagebtn_Next">Next</a></li>';
                    $('#' + pagerid + ' #jqpagebtn_1').addClass("jqactive");

                    $('#' + pagerid + ' #first_' + pagerid).remove();
                    //$('#' + pagerid + ' #prev_pager').remove();
                    //$('#' + pagerid + ' #next_pager').remove();
                    $('#' + pagerid + ' #prev_' + pagerid).html('Previous');
                    $('#' + pagerid + ' #next_' + pagerid).html('Next');
                    $('#' + pagerid + ' #last_' + pagerid).remove();
                    //}
                    ////added for handling next and prev clicks
                    $('#gview_' + gridid + ' #jqpagebtn_Previous').live('click', function () {

                        var page = 0;
                        page = parseInt($('#' + pagerid + ' .ui-pg-input').val());
                        if (isNaN(page)) {
                            page = 1;
                        }
                        if (page != 1) {
                            page -= 1;
                        }
                        SetGridPage(page, pagerid);
                    });

                    $('#gview_' + gridid + ' #jqpagebtn_Next').live('click', function () {

                        var page = 0;
                        page = parseInt($('#' + pagerid + ' .ui-pg-input').val());
                        if (isNaN(page)) {
                            page = 1;
                        }
                        if (pagecnt > page) {
                            page += 1;
                        }
                        SetGridPage(page, pagerid);
                    });
                    ////end                                                

                    if ($('#' + pagerid + ' .ui-paging-info').html() == null || $('#' + pagerid + ' .ui-paging-info').html() == 'No records to view') {
                        var newDivPaging = divPaging.replace('@rowmsg', "");
                        norecord = '<tr class="no_record"><td colspan="' + $('#' + gridid + ' .jqgfirstrow td').size() + '">' +
                                    $('#' + pagerid + ' .ui-paging-info').html() + '</td></tr>';
                    }
                    else {
                        var newDivPaging = divPaging.replace('@rowmsg', $('#' + pagerid + ' .ui-paging-info').html());
                    }
                    newDivPaging = newDivPaging.replace('@selectpage', "<select id='selPageSize'>" + $('#' + pagerid + ' .ui-pg-selbox').html() + "</select>");
                    newDivPaging = newDivPaging.replace('@paging', "<ul>" + pagelist + "</ul>");
                    pagebar = '<tr class="trPaging"><td colspan="' + $('#' + gridid + ' .jqgfirstrow td').size() + '">' +
                newDivPaging
                + '</td></tr>';
                }
                //Number JqGrid Paging end

                ////Appending header in list table (to handle width issue)
                $('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"] tbody').remove();
                $('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"]').append($('<tbody></tbody>')).append($('#' + gridid + ' tbody tr:not(.jqgfirstrow)'));
                if (norecord.length > 0)
                    $('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"] tbody').append($(norecord));
                if (pagebar.length > 0)
                    $('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"] tbody').append($(pagebar));
                $('#' + ((pagerid == '') ? ('p' + gridid) : pagerid)).css('display', 'none');

                if (pagerid != '') {
                    var pg = parseInt($('#' + pagerid + ' .ui-pg-input').val());
                    $('#gview_' + gridid + ' #jqpagebtn_' + pg).addClass("on");
                    $('#gview_' + gridid + ' #selPageSize').bind('change', function () {

                        $('#' + pagerid + ' .ui-pg-selbox').val($('#gview_' + gridid + ' #selPageSize').val());
                        $('#' + pagerid + ' .ui-pg-selbox').trigger('change');
                    });

                    var currentPager = $('#' + pagerid + ' .ui-pg-selbox').val();
                    //console.log(currentPager);
                    if (document.URL.toLowerCase().indexOf('/suborganizationlist') != -1) ///to handle multiple custom select problem 
                    {
                        $('#gview_' + gridid + ' #selPageSize').val(currentPager);
                        $.each($('#selPageSize').parent(), function (key, value) {
                            $(this).find('span').html($(this).find('select#selPageSize option:selected').val());
                        });
                    }
                    else
                        $('#gview_' + gridid + ' #selPageSize').val(currentPager);
                    $('#load_' + gridid).css('display', 'none');
                    if ($('#' + pagerid + ' #prev_' + pagerid).hasClass('ui-state-disabled'))
                    { $('#gview_' + gridid + ' #jqpagebtn_Previous').addClass('ui-state-disabled').on('click', function () { return false; }); }
                    else
                    { $('#gview_' + gridid + ' #jqpagebtn_Previous').removeClass('ui-state-disabled'); }

                    if ($('#' + pagerid + ' #next_' + pagerid).hasClass('ui-state-disabled'))
                    { $('#gview_' + gridid + ' #jqpagebtn_Next').addClass('ui-state-disabled').on('click', function () { return false; }); }
                    else
                    { $('#gview_' + gridid + ' #jqpagebtn_Next').removeClass('ui-state-disabled'); }

                    if ($('tr.no_record').size() > 0)
                    { $('#gview_' + gridid + ' #jqpagebtn_Next').addClass('ui-state-disabled').on('click', function () { return false; }); }
                    else
                    { $('#gview_' + gridid + ' #jqpagebtn_Next').removeClass('ui-state-disabled'); }
                }
                //$('.jump_page').jqTransform({ imgPath: 'Content/Images/' });
                //bindCustomDropdown('#gview_' + gridid + ' .jump_page');

                //Added by Rachit for ADSection to list groups after gridLoad
                if (document.URL.toLowerCase().indexOf('/aduserlist') != -1)
                    AllGroupData();

                //$('#pager').append('<div class="clr"></div><div class="space80"></div>');


                //--------------------------------------------------------------------------
                //After load the all data this will used to check the checkbox, which is
                //selected previuosly
                if (jsonCheckData != null && jsonCheckData.length > 0) {
                    for (var i in jsonCheckData) {
                        if ($('.ui-jqgrid-htable tr').find('td[title="' + jsonCheckData[i].Id + '"][aria-describedby="list_ID"]').text() === jsonCheckData[i].Id) {
                            var chkId = $('.ui-jqgrid-htable tr').find('td[title="' + jsonCheckData[i].Id + '"]').prev().find('input').attr('id');
                            $('#' + chkId).attr('checked', 'checked');
                        }
                    }
                }
                //--------------------------------------------------------------------------

                $('.ui-jqgrid-htable > tbody > tr').each(function () {
                    //alert($(this).find('td[aria-describedby="list_ID"]').attr('title'));
                    if (!isNaN($(this).find('td[aria-describedby="list_Id"]').attr('title'))) {
                        var id = $(this).find('td[aria-describedby="list_Id"]').attr('title');
                        var UserType = $(this).find('td[aria-describedby="list_Type"]').attr('title');
                        jqGrigIDs.push(id.toString());
                        if (UserType == "NU")
                        {
                          //  alert(id);
                            jqGridNuIDs.push(id.toString());
                        }
                        if (UserType == "CP") {
                          //  alert(id);
                            jqGridCPIDs.push(id.toString());
                        }
                        if (UserType == "CA") {
                          //  alert(id);
                            jqGridCAIDs.push(id.toString());
                        }
                        if (UserType == "CC") {
                           // alert(id);
                            jqGridCCIDs.push(id.toString());
                        }
                        if (UserType == "CK") {
                           // alert(id);
                            jqGridCKIDs.push(id.toString());
                        }
                        if (UserType == "SC") {
                            // alert(id);
                            jqGridSUPIDs.push(id.toString());
                        }
                    }
                });
            },
        onSelectRow: function (rowid) {
            var celValue = $('#' + gridid).jqGrid('getCell', rowid, 'DoActive');
            var type = $('#' + gridid).jqGrid('getCell', rowid, 'Type');
            if (celValue == "false" && type == "true") {
                $('#' + gridid).jqGrid('setSelection', rowid, false);
            }
            var cbs = $("tr#" + rowid + " > td > input.cbox:disabled", $('#' + gridid)[0]);
            if (cbs.length > 0) {
                cbs.removeAttr("checked");
                $('#' + gridid).jqGrid('setSelection', rowid, false);
            }

            $('.ui-jqgrid-htable > tbody > tr').each(function () {
                if ($(this).find('td[aria-describedby="list_Enrolled"]').attr('title') == 'No') {
                    $(this).find('td[aria-describedby="list_Enrolled"]').parent().removeClass('selected-row');
                }
            });
        },
        onSelectAll: function (aRowids, status) {
            $("#gview_" + gridid + " tr td > input.cbox").removeAttr('checked').parent().parent().removeClass('selected-row');
            if (status) {
                $("#gview_" + gridid + " tr td > input.cbox[disabled!='disabled']").attr('checked', 'checked').parent().parent().addClass('selected-row');
            }

            $('.ui-jqgrid-htable > tbody > tr').each(function () {
                if ($(this).find('td[aria-describedby="list_Enrolled"]').attr('title') == 'No') {
                    $(this).find('td[aria-describedby="list_Enrolled"]').parent().removeClass('selected-row');
                }
            });
        },
        gridComplete: function (data) {
            $("#divProgress").hide();
            bindCustomDropdown('#gview_' + gridid + ' .jump_page');
        }
    }).navGrid('#' + pagerid, { edit: editOn, add: false, del: false, search: false, refresh: false },
            {
                width: 'auto', beforeShowForm: function () {
                    $('.navButton').hide();
                    var dlgDiv = $("#editmod" + gridid);
                    var parentDiv = dlgDiv.parent(); // div#gbox_list
                    var dlgWidth = dlgDiv.width();

                    var parentWidth = parentDiv.width();
                    var dlgHeight = dlgDiv.height();
                    var parentHeight = parentDiv.height();
                    // TODO: change parentWidth and parentHeight in case of the grid
                    //       is larger as the browser window
                    dlgDiv[0].style.top = Math.round(((parentHeight - dlgHeight) / 2) + 180) + "px";
                    dlgDiv[0].style.left = Math.round((parentWidth - dlgWidth) / 2) + "px";
                    dlgDiv[0].style.width = '400px';
                    ViewSetting();
                }, url: '@Url.Action("InsertAdminUser", "AdminUser")', modal: true, closeAfterEdit: true, closeAfterAdd: true, afterSubmit: processAddEdit
            });

    $(".jqGrid_refresh").click(function () {
        //$("#divProgress").show();
        $('#' + gridid).trigger('reloadGrid', [{ page: 1 }]);
    });

    $(".jqGrid_refreshKey").keypress(function (args, event) {
        if (args.keyCode == 13)
            $('#' + gridid).trigger('reloadGrid', [{ page: 1 }]);
    });

    $("#aDelete").click(function () {

        $('#del_jqgAdminUser').trigger('click');
    });

    $('#' + gridid).fluidGrid({ example: "content_div_new", offset: -10 });
    $('.navtable').remove();
}

$("tbody input[type='checkbox']").live('change', function () {
    ////console.log('in');
    var flg = $(this).is(":checked");
    if (flg)
        $(this).parent().parent().addClass('selected-row');
    else
        $(this).parent().parent().removeClass('selected-row');

    if ($(this).parent().parent().find('td[aria-describedby="list_Enrolled"]').attr('title') == 'No') {
        $(this).parent().parent().find('td[aria-describedby="list_Enrolled"]').parent().removeClass('selected-row');
    }
});

function SetCurrency(cellvalue, options, rowObject) {
    try {

        if (isNaN(cellvalue) || cellvalue == null) {
            cellvalue = 0;
        }
        return $("#JqCurrencySymbol").val() + cellvalue;
    }
    catch (ex) { alert(ex.message); }
}

function bindJqGridMin(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title, loadfunc) {

    bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', loadfunc, false, true, null, null, null);
}

function bindJqGridMinWithGrouping(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title, allowGrouping, groupModel) {

    bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', null, false, true, allowGrouping, groupModel, null);
}

function bindJqGridEdit(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title, loadfunc) {

    bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', loadfunc, true, true, null, null, null);
}

function bindJqGridNoMultiSelect(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title, loadfunc) {

    bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', loadfunc, false, false, null, null, null);
}

function bindJqGridMinWithCustomPaging(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title, loadfunc, paging) {
    if (gridid == 'listSubOrg') {
        bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', loadfunc, false, false, null, null, paging);
    }
    else {
        bindJqGrid(gridid, pagerid, url, columns, model, sorton, sortdir, postData, title,
        'json', 'GET', loadfunc, false, true, null, null, paging);
    }
}

function SetViewLink(cellvalue, options, rowObject) {
    try {
        var rowid = rowObject["ID"];
        var onclickCall = 'onclick=javascript:get(' + rowid + ')';
        //        if (type == "true" || type == true) {
        return "<a " + onclickCall + " href='javascript:void(0)' >" + cellvalue + "</a>";
        //        }
        //        else
        //            return "";

    }
    catch (ex) { alert(ex.message); }
};

function showActionInactMsg() {
    showStickMessage('You are unautherized to perform this action.');
    return false;
}

SetActiveInActive = function (cellvalue, options, rowObject) {
    try {

        // debugger;
        var rowId = rowObject["ID"];
        var type = rowObject["Type"];
        var Grouptype = rowObject["Grouptype"];


        var onclickCall = 'onclick="javascript:UpdateRowStatus(' + rowId + ',' + cellvalue + ');"';
        // if (($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True") && !notadminpanel) {
        if (($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True")) {
            //onclickCall = 'onclick="javascript:showStickMessage("unaccess"); return false;"';
            onclickCall = 'onclick="javascript:return NonAccessMsg(); "';
        }
        else {

            var MainParentOrgStatus = rowObject["MainParentOrgStatus"];


            if (MainParentOrgStatus != undefined) {
                if (MainParentOrgStatus == 'I') {
                    onclickCall = 'onclick="javascript:showMainOrgMsg(); return false;"';
                    //return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to activate' src='/Content/images/nw_inactive.png'/></a>";
                    return '<div class="selected-row"><div class="switch"><input id="cmn-toggle-' + rowId + '" class="cmn-toggle cmn-toggle-round" type="checkbox" ' + onclickCall + '><label for="cmn-toggle-' + rowId + '"></label></div></div>';
                }
            }
        }
        //
        var grouptype = (Grouptype != undefined) ? Grouptype : true;
        if (grouptype)
            grouptype = rowObject["Grouptype"];

        var doactive = (type != undefined) ? type : true;
        if (doactive)
            doactive = rowObject["DoActive"];
        if (doactive == "true" || doactive == true) {// || doactive == false || doactive == "false") {
            if (cellvalue == "true" || cellvalue == true)
                //return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to deactivate' src='/Content/images/nw_active.png'/></a>";
                return '<div class="selected-row"><div class="switch"><input id="cmn-toggle-' + rowId + '" class="cmn-toggle cmn-toggle-round" type="checkbox" ' + onclickCall + ' checked="checked"><label for="cmn-toggle-' + rowId + '"></label></div></div>';
            else
                //return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to activate' src='/Content/images/nw_inactive.png'/></a>";
                return '<div class="selected-row"><div class="switch"><input id="cmn-toggle-' + rowId + '" class="cmn-toggle cmn-toggle-round" type="checkbox" ' + onclickCall + '><label for="cmn-toggle-' + rowId + '"></label></div></div>';
        }
        else {
            return "";
        }


    }
    catch (ex) {
        alert(ex.message);
    }
}


SetActiveInActiveGroup = function (cellvalue, options, rowObject) {
    try {

        //debugger;
        var rowId = rowObject["ID"];
        var type = rowObject["Type"];
        var Grouptype = rowObject["Grouptype"];


        var onclickCall = 'onclick="javascript:UpdateRowStatus(' + rowId + ',' + cellvalue + ');"';
        if (($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True") && !notadminpanel) {
            onclickCall = 'onclick="javascript:showStickMessage(unaccess); return false;"';
        }
        else {

            var MainParentOrgStatus = rowObject["MainParentOrgStatus"];


            if (MainParentOrgStatus != undefined) {
                if (MainParentOrgStatus == 'I') {
                    onclickCall = 'onclick="javascript:showMainOrgMsg(); return false;"';
                    return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to activate' src='/Content/images/nw_inactive.png'/></a>";
                }
            }
        }
        //
        var grouptype = (Grouptype != undefined) ? Grouptype : true;
        if (grouptype)
            grouptype = rowObject["Grouptype"];

        var doactive = (type != undefined) ? type : true;
        if (doactive)
            doactive = rowObject["DoActive"];
        if ((doactive == "true" || doactive == true) && (grouptype == "Database")) {// || doactive == false || doactive == "false") {
            if (cellvalue == "true" || cellvalue == true)
                return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to deactivate' src='/Content/images/nw_active.png'/></a>";
            else
                return "<a href='#'><img " + onclickCall + " alt='" + cellvalue + "' title='click to activate' src='/Content/images/nw_inactive.png'/></a>";
        }
        else {
            return "";
        }


    }
    catch (ex) {
        alert(ex.message);
    }
}


SetLocation = function (cellvalue, options, rowObject) {
    try {

        var eid = rowObject["EID"];
        return "<a href='LocationList/" + eid + "'>Create</a>";
    }
    catch (ex) {
        alert(ex.message);
    }
}



SetAppStoreLink = function (cellvalue, options, rowObject) {
    try {

        var rowId = rowObject["AppStoreLink"];
        return "<a href='" + rowId + "' target='_blank'>Open App Store</a>";
    }
    catch (ex) {
        alert(ex.message);
    }
}

SetAdminLink = function (cellvalue, options, rowObject) {
    try {

        var link = rowObject["AppStoreLink"];
        return '<a href =' + link.replace("Store/Home", "AppStore/organization/LogIn") + '>Open Admin Link</a>';
    }
    catch (ex) {
        alert(ex.message);
    }
}

SetDeptLink = function (cellvalue, options, rowObject) {
    try {

        var rowId = rowObject["ID"];
        return "<a href='javascript:;' onclick='EditMode(" + rowId + ");'>Edit</a>&nbsp;&nbsp;<a href='javascript:;' onclick='DeleteDepartment(" + rowId + ");'>Delete</a>";
    }
    catch (ex) {
        alert(ex.message);
    }
}

function UpdateStatus(_action, _controller, _id, _status, gridId) {
    try {
        $.post('/' + _controller + '/' + _action, { id: _id, status: _status }, function (data) {
            var pagenum = $('#' + gridId).getGridParam('page');
            $('#' + gridId).trigger('reloadGrid', [{ page: pagenum }]);
            CallafterSubmit(data, null, _status);
        });
    }
    catch (ex) { alert(ex.message); }
}

function CallafterSubmit(response, postdata, actStatus) {

    var message = '';
    var status = response.statusText;
    var module = response.module;
    var task = response.task;

    if (actStatus != null)
        if (actStatus == false)
            task = "active";
        else
            task = "deactive";

    switch (status != null && status.toLowerCase()) {
        case "success":
            if (response.errormessage != null && response.errormessage.length > 0) {
                message = response.errormessage;
                showStickMessage(message);
                break;
            }
            else if (response.message != null && response.message.length > 0)
                message = response.message;
            else
                message = GetSuccessMessage(task).replace("@module", module);
            showMessage(message);

            break;
        case "error":
            if (response.message != null && response.message.length > 0)
                message = response.message;
            else
                message = GetErrorMessage(task).replace("@module", module);
            showStickMessage(message);
            break;
        case "cannotdelete":
            message = "Selected App(s) are linked with Ticket(s), so could not be deleted. Other deleted successfully.";
            showStickMessage(message);
            break;
        case "cannotdeleteVouchers":
            if (response.message != null && response.message.length > 0)
                message = response.message;
            else
                message = GetErrorMessage(task).replace("@module", module);
            showStickMessage(message);

            //        alert("je");
            //        message = "";
            //        showStickMessage(message);
            break;
    }
}

function GetSelectedGridIds(gridid) {
    return $("#gview_" + gridid + " tr:has(td > input.cbox:checked[disabled!='disabled'])").
    find("td[aria-describedby='" + gridid + "_ID']").map(function () {
        return $(this).attr('title');
    }).get();
}

function DeleteMultiple(gridid, posturl) {
    var selids = GetSelectedGridIds(gridid);
    DeleteMultipleMethod(gridid, posturl, selids);
}

function DeleteMultipleMethod(gridid, posturl, selids) {
    var count = selids.length;
    if (count == 0) { alert("Select alteast one record to delete."); return; }
    else if (count > 0) {
        if (count == 1) {
            if (confirm("Are you sure you want to delete this " + count + " record(s)?")) {
                jsonCheckData = []; //Clear selected data from JSON object.
                deleteData(gridid, posturl, selids);
            }
        }
        else {
            if (confirm("Are you sure you want to delete these " + count + " record(s)?")) {
                jsonCheckData = []; //Clear selected data from JSON object.
                deleteData(gridid, posturl, selids);
            }
        }
    }
}


function deleteData(gridid, posturl, selids) {
    $.ajax({
        type: "POST",
        url: posturl,
        datatype: "json",
        traditional: true,
        data: { 'ids': selids },
        success: function (response) {
            $("#" + gridid).trigger("reloadGrid");
            CallafterSubmit(response, null, null);
            jsonCheckData = [];
        }
    });
}


function UpdateApproveStatus(status, gridid, posturl) {
    var grid = $("#" + gridid);
    if (!grid.jqGrid) {

    }
    var selids = GetSelectedGridIds(gridid);
    UpdateApproveStatusMultiple(gridid, posturl, selids, status);
}

function UpdateEndUserDepartment(depid, gridid, posturl) {

    var grid = $("#" + gridid);
    if (!grid.jqGrid) {

    }

    var selids = GetSelectedGridIds(gridid);

    UpdateDepartmentMultiple(gridid, posturl, selids, depid);
}

function UpdateDepartmentMultiple(gridid, posturl, selids, depid) {
    var count = selids.length;
    if (count == 0) { showStickMessage("Select alteast one record to update."); $("#divProgress").hide(); return; }
    if (confirm("Are you sure you want to update Department of these " + count + " user(s)?")) {
        jsonCheckData = [];
        $("#divProgress").show();
        $.ajax({
            type: "POST",
            url: posturl,
            datatype: "json",
            traditional: true,
            data: { 'ids': selids, 'depid': depid },
            success: function (response) {
                $("#" + gridid).trigger("reloadGrid");
                CallafterSubmit(response, null, null);
            }
        });

    }
    else {
        $("#divProgress").hide();
    }
}


function UpdateApproveStatusMultiple(gridid, posturl, selids, status) {
    var count = selids.length;
    if (count == 0) { alert("Select alteast one record to update."); return; }
    if (confirm("Are you sure you want to update status of these " + count + " record(s)?")) {
        jsonCheckData = [];
        $.ajax({
            type: "POST",
            url: posturl,
            datatype: "json",
            traditional: true,
            data: { 'ids': selids, 'status': status },
            success: function (response) {
                $("#" + gridid).trigger("reloadGrid");
                CallafterSubmit(response, null, null);
            }
        });
    }
}



function GetSuccessMessage(task) {
    switch (task.toLowerCase()) {
        case "update":
            return "@module updated Successfully";
            break;
        case "delete":
            return "@module deleted Successfully";
            break;
        case "active":
            return "@module Activated Successfully";
            break;
        case "deactive":
            return "@module Deactivated Successfully";
            break;
        case "remove":
            return "@module Removed Successfully";
            break;
    }
    return "";
}

function GetErrorMessage(task) {
    switch (task.toLowerCase()) {
        case "update":
            return "Error occured while updating @module";
            break;
        case "delete":
            return "Error occured while deleting @module";
            break;
        case "active":
            return "Error occured while activating @module";
            break;
        case "deactive":
            return "Error occured while deactivating @module";
            break;
        case "select":
            return "No record is selected";
            break;
        case "remove":
            return "Error occured while removing @module";
            break;
    }
    return "";
}

function SetViewLink(cellvalue, options, rowObject) {

    try {
        var id = options["rowId"];
        var onclickCall = 'onclick=javascript:get(' + id + ',"' + options["gid"] + '")';
        return "<a " + onclickCall + " href='javascript:void(0)' >View</a>";
    }
    catch (ex) { alert(ex.message); }
};

function get(rowid, gridId) {

    var grid = jQuery('#' + gridId);
    grid.jqGrid('resetSelection');
    grid.jqGrid('setSelection', rowid);
    $('#edit_' + gridId).trigger('click');
}

function processAddEdit(response, postdata) {
    try {
        if (response.statusText == "success") {
            /**********************************/
            var success = true;
            var message = ""
            var json = eval('(' + response.responseText + ')');
            if (json.failed) {
                success = false;
                message = json.failed + '<br/>';
            }
            if (json.errors) {
                success = false;
                for (i = 0; i < json.errors.length; i++) {
                    message += json.errors[i] + '<br/>';
                }
            }
            else if (json.statusText == "success")
            { CallafterSubmit(response, postdata, null); }
            var new_id = "1";
            return [success, message, new_id];
            /*********************************/
        }
    }
    catch (ex) {
        alert('processAddEdit--' + ex.message);
    }
}

function SetGridPage(page, currentPagerName) {
    var pagerID = '';
    try {
        pagerID = currentPagerName.id;
        if (pagerID == null || pagerID == '')
            pagerID = currentPagerName;
    }
    catch (e) {
        pagerID = currentPagerName;
    }
    //$("#divProgress").show();
    $('#' + pagerID + ' input.ui-pg-input').val(page);
    $('#' + pagerID + ' .jqpagebtn').removeClass("jqactive");
    $('#' + pagerID + ' #jqpagebtn_' + page).addClass("jqactive");
    var e = jQuery.Event("keypress");
    e.which = 13;
    e.keyCode = 13;
    //$('#' + currentPagerID + ' input.ui-pg-input').size();
    $('#' + pagerID + ' input.ui-pg-input').trigger(e);
    //$('input.ui-pg-input').trigger(e);
}

function showMainOrgMsg() {
    alert("This user is De-activated by Main Organization.");
}

///setting sorting icon here - Rachit
//setSortIcon();
//function setSortIcon() {
//    var spnHtml = '';
//    var divHtml = '';
//    $('.s-ico').each(function (index) {
//        //index
//        CHtml = this.html();
//          this.remove();
//        divHtml = this.parent().html();
//this.parent.html().remove();
//        this.parent.append('<table><tr><td></td></tr></table>')
//    });
//}




/////////----------Old jg grid paging ------------------
////Number JqGrid Paging Start
//var pagecnt = $('#sp_1_pager').html();
//var previouscnt = $('.jqpagebtn:last').text();
//if (pagecnt != previouscnt) {

//    var inptPage = $('td[dir="ltr"]:first').find('input');

//    inptPage.css('display', 'none');
//    $('#sp_1_pager').css('display', 'none');

//    $('.jqpagebtn').remove();
//    for (var i = 1; i <= pagecnt; i++)
//        $('td[dir="ltr"]:first').append("<button onclick='SetGridPage(" + i + ")' class='jqpagebtn' id='jqpagebtn_" + i + "' currentPage=" + i + ">" + i + "</button>");

//    $('#jqpagebtn_1').addClass("jqactive");

//    $('#first_pager').remove();
//    //$('#prev_pager').remove();
//    //$('#next_pager').remove();
//    $('#prev_pager').html('Previous');
//    $('#next_pager').html('Next');
//    $('#pager_left').html($('#pager_right').html());
//    $('#pager_right').html('');
//    $('#last_pager').remove();
//    ////added for handling next and prev clicks
//    $('#prev_pager').bind('click', function () {
//        var page = 0;
//        page = parseInt($('.jqactive').attr('currentPage'));
//        if (isNaN(page)) {
//            page = 1;
//        }
//        if (page != 1) {
//            page -= 1;
//        }
//        console.log(page);
//        SetGridPage(page);
//    });

//    $('#next_pager').bind('click', function () {
//        var page = 0;
//        page = parseInt($('.jqactive').attr('currentPage'));
//        if (isNaN(page)) {
//            page = 1;
//        }
//        if (pagecnt > page) {
//            page += 1;
//        }
//        console.log(page);
//        SetGridPage(page)
//    });
//    ////end

//}
////                    //Number JqGrid Paging end

//////Appending header in list table (to handle width issue)
//$('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"] tbody').remove();
//$('table.ui-jqgrid-htable[aria-labelledby="gbox_' + gridid + '"]').append($('<tbody></tbody>')).append($('#' + gridid + ' tbody tr'));
//$('#' + gridid + ' tbody tr').hide();