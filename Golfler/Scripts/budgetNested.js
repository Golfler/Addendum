
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
        bindJqGridNoMultiSelect('sublist' + id, '', $('#getsublisturl').val(),
                [  'Org. Name', 'Name', 'Total Amount Assigned($)', 'Amount Used for Distribute($)','Amount Used for Apps($)' , 'Balance Amount'],
                [ 
                    { name: 'OrgName', index: 'OrgName', width: 200, sortable: false },
                     { name: 'Name', index: 'Name', width: 200, sortable: false },
                      { name: 'AllocatedAmount', index: 'AllocatedAmount', width: 200, sortable: false },
                         { name: 'DistributedAmount', index: 'DistributedAmount', width: 90, sortable: false },
                        { name: 'amountapps', index: 'amountapps', width: 90, sortable: false },
                         { name: 'AmountBalace', index: 'AmountBalace', width: 90, formatter: SetAmountBalace, sortable: false }


                    ],
                    'BudgetTrackId', 'desc',
                    {
                        "cuserid": (function () {
                            return id;
                        }),
                        "searchText": ""
                    }, '');
        $('#tr' + id).attr('aria-selected', "true");
        $('#tr' + id).toggle();
        $('#a' + id).parent().parent().addClass('haschild');
    }
}
function SetAmountBalace(cellvalue, options, rowObject) {
    try {

        var actualamt = rowObject["AllocatedAmount"];

        var DistributedAmount = rowObject["DistributedAmount"];
        var amountapps = rowObject["amountapps"];
        var totalused = DistributedAmount + amountapps;

        return actualamt - totalused;
    }
    catch (ex) { alert(ex.message); }
}