
//Organization/ Sub Organization List
function BindInnerGrid(id, voucherid) {
    if ($('#tr' + id + ' .sublist').hasClass('ui-jqgrid-btable')) {
        $('#a' + id).html(openChild);
        $('#tr' + id).children().remove();
        $('#tr' + id).append('<td colspan="9" style="padding: 5px;"><div id="div' + id + '" style="width: 99%;"><table class="sublist" id="sublist' + id + '"></table><div id="psublist' + id + '"></div></div></td>');
        $('#a' + id).parent().parent().removeClass('haschild');
        $('#tr' + id).removeAttr('aria-selected');
        $('#tr' + id).toggle();
    }
    else {
        $('#a' + id).html(closeChild);
        var show = $('#hdSuper').size() == 0;
        //Added by Rachit for MasterView SubOrg Section
        var subOrgListFlag = false;
        var type = '';
        if (document.URL.toLowerCase().indexOf('/suborganizationlist') != -1)
        { subOrgListFlag = true; type = 'suborganizationlist'; }
        else if (document.URL.toLowerCase().indexOf('/aduserlist') != -1)
        { subOrgListFlag = true; type = "aduserlist"; }

        if (!subOrgListFlag) {
            bindJqGridNoMultiSelect('sublist' + id, '', $('#getsublisturl').val(),
                ['ID', 'Sub Org Name', 'App Store Link', 'Add Credits',
                'Action',
                'Tenant ID', 'Created Date', 'Active', 'DoActive'],
                [{ name: 'ID', index: 'ID', hidden: true },
                    { name: 'Name', index: 'Name', width: 200, formatter: SetEditLink },
                    { name: 'AppStoreLink', index: 'AppStoreLink', sortable: false, formatter: SetAppStoreLink, align: "center" },
                    { name: 'Add Credits', index: 'Add Credits', sortable: false, formatter: SetAddCreditsLink, align: "center", hidden: show },
                    { name: 'Action', index: 'Action', sortable: false, formatter: SetSubOrgLink, align: "center" },
                    { name: 'Create', index: 'Create', sortable: false, formatter: isTanentID, align: "center" },
                    { name: 'CreatedDate', index: 'CreatedDate', editable: true, formatter: 'date', formatoptions: { newformat: dateformat} },
                    { name: 'Active', index: 'Active', sortable: false, formatter: _SetActiveInActive, align: "center" },
                    { name: 'DoActive', index: 'DoActive', hidden: true}],
                    'ID', 'desc',
                    {
                        "orgid": (function () {
                            return id;
                        }),
                        "searchText": ""
                    }, '');
        }
        else if (type == 'suborganizationlist') { ///binding subOrg for master view here
            bindJqGridNoMultiSelect('sublist' + id, '', $('#getsublisturl').val(),
                ['ID', 'Sub Org Name', 'Sub Org Head'],
                [{ name: 'ID', index: 'ID', hidden: true },
                    { name: 'Name', index: 'Name', width: 200, formatter: SetDDlLink },
                    { name: 'AdminName', index: 'AdminName', width: 200}],
                    'ID', 'desc',
                    {
                        "orgid": (function () {
                            return id;
                        }),
                        "searchText": ""
                    }, '');
        }
        else if (type == 'aduserlist') { ///binding subOrg for master view here
            bindJqGridNoMultiSelect('sublist' + id, '', $('#getsublisturl').val(),
                ['ID', 'Sub Org Name'],
                [{ name: 'ID', index: 'ID', hidden: true },
                    { name: 'Name', index: 'Name', width: 200, formatter: SetDDlLink}],
                    'ID', 'desc',
                    {
                        "orgid": (function () {
                            return id;
                        }),
                        "searchText": ""
                    }, '');
        }
        $('#tr' + id).attr('aria-selected', "true");
        $('#tr' + id).toggle();
        $('#a' + id).parent().parent().addClass('haschild');
    }
}

function _SetActiveInActive(cellvalue, options, rowObject) {
    try {
        var rowId = rowObject["ID"];

        var onclickCall = 'onclick="javascript:UpdateRowStatus(' + rowId + ',' + cellvalue + ');"';
        if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True")
        { onclickCall = 'onclick="javascript:showStickMessage(unaccess); return false;"'; }

        doactive = rowObject["DoActive"];
        if ((doactive == "true" || doactive == true)) {
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

function SetEditLink(cellvalue, options, rowObject) {
    try {
        //var EID = rowObject["EID"];
        //return "<a href='EditOrg/" + EID + "' >" + cellvalue + "</a>";

        var Eid = rowObject["EID"];
        var type = rowObject["Type"];
        var doActive = rowObject["DoActive"];
        if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True")
            return cellvalue;
        else
            return ((type == true || type == "true") || (doActive == "false" || doActive == false)) ? "<a href='EditOrg/" + Eid + "' >" + cellvalue + "</a>" : cellvalue;

    }
    catch (ex) { alert(ex.message); }
}


function SetOrgLink(cellvalue, options, rowObject) {
    try {
        var rowid = rowObject["ID"];
        return "<a href='javascript:;' onclick='Open(" + rowid + ")'>Login as Organization Admin</a>";
    }
    catch (ex) { alert(ex.message); }
}

function Open(id) {
    $.ajax({
        type: 'POST',
        url: '/Organization/SuperAdminLogin',
        data: { orgid: id },
        success: function (data) {
            if (data.msg == true) {
                window.location.href = $('#sublisturl').val() + 'organization/dashboard';
            }
            else {
                showStickMessage('Not able to login. Try again');
            }
        }
    });
}


function isTanentID(cellvalue, options, rowObject) {
    try {
        var hasTenantId = rowObject["hasTenantId"];
        var orgID = rowObject["ID"];
        if (!hasTenantId) {
            var onclickCall = 'onclick="javascript:GenerateTanentId(' + orgID + ');"';
            return "<a " + onclickCall + " href='#' >Generate</a>";
        }
        else
            return rowObject["TenantId"];
    }
    catch (ex) { alert(ex.message); }
}


function GenerateTanentId(id) {
    $.post('/Organization/GenerateTanentId', { orgID: id }, function (data) {
        var pagenum = $('#list').getGridParam('page');
        $('#list').trigger('reloadGrid', [{ page: pagenum}]);
        CallafterSubmit(data, null, _status);
    });
}

function SetAddCreditsLink(cellvalue, options, rowObject) {
    try {
        if ($('#hdSuper').size() > 0) {
            var eid = rowObject["EID"];
            var OrgaName = rowObject["Name"];
            return "<a href='AddCredits?OrgaName=" + OrgaName + "&id=" + eid + "' >Add Credits</a>";
        }
        return "";
    }
    catch (ex) { alert(ex.message); }
}

function SetSubOrgLink(cellvalue, options, rowObject) {
    try {
        var rowid = rowObject["ID"];
        return "<a href='javascript:;' onclick='Open(" + rowid + ")' class='subOrg'>Login as Sub-Organization Admin</a>";

    }
    catch (ex) { alert(ex.message); }
}

function SetDDlLink(cellvalue, options, rowObject) {
    try {
        var rowid = rowObject["ID"];
        return "<a href='javascript:;' subOrgName='" + cellvalue + "' subOrgID='" + rowid + "' class='subOrgan' title='Click here to select!'>" + cellvalue; +"</a>";
    }
    catch (ex) { alert(ex.message); }
}