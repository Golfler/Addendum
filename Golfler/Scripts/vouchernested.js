
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
                ['VoucherUserId', 'Org. Name', 'Name', 'Total Amount Assigned ($)','Amount Used for Distribute ($)','Amount Used for Codes ($)','Balance Amount'],
                [{ name: 'VoucherUserId', index: 'VoucherUserId',hidden:true },
                 { name: 'OrgName', index: 'OrgName', width: 100, sortable: false },
                 { name: 'Name', index: 'Name', width: 100, sortable: false },
                 { name: 'ActualAmount', index: 'ActualAmount', width: 50, sortable: false },
                 { name: 'DistributedAmount', index: 'DistributedAmount', formatter: SetAmountDistribute, width: 50, sortable: false },
                 { name: 'amountcodes', index: 'amountcodes', width: 50, sortable: false },
                 { name: 'AmountBalace', index: 'AmountBalace', width: 50, formatter: SetAmountBalace, sortable: false }
                ],
                    'VoucherUserId', 'desc',
                    {
                        "orgid": (function () {
                            return id;
                        }),
                        "voucherid": (function () {
                            return voucherid;
                        }),
                        "searchText": ""
                    }, '');
        $('#tr' + id).attr('aria-selected', "true");
        $('#tr' + id).toggle();
        $('#a' + id).parent().parent().addClass('haschild');
    }
}
function SetAmountDistribute(cellvalue, options, rowObject) {
    try {
        var ActualAmount = rowObject["ActualAmount"];
        var Amount = rowObject["Amount"];
        var DistributedAmount = ActualAmount - Amount;
        return DistributedAmount;
        //var amountcodes = rowObject["amountcodes"];
        // return amountcodes + DistributedAmount;
    }
    catch (ex) { alert(ex.message); }
}
function SetAmountBalace(cellvalue, options, rowObject) {
    try {


        var actualamt = rowObject["ActualAmount"];
        var Amount = rowObject["Amount"];

        var DistributedAmount = actualamt - Amount;

        var amountcodes = rowObject["amountcodes"];
        var totalused = amountcodes + DistributedAmount;

        return actualamt - totalused;
    }
    catch (ex) { alert(ex.message); }
}