/// <summary>
/// Created By: Renuka
/// Creation On: 3June, 2013
/// Description: for javascrit common methods
/// </summary>
var dateformat;

(function ($) {
    var originalVal = $.fn.val;
    $.fn.val = function (value) {
        //alert(value + 'get');
        if (typeof value != 'undefined') {
            // setter invoked, do processing
            if (this[0] != undefined && this[0].nodeName == 'SELECT') {
                if ($(this).attr('id') != 'Mnu')
                    $("#" + $(this).attr('id')).parent().find('span').html($("#" + $(this).attr('id') + " option[value='" + value + "']").text());
            }
            return originalVal.call(this, value);
        }
        return originalVal.call(this);
    };
})(jQuery);

$(function () {
    try
    {
        dateformat = $('#JqDateFormat').val();
    }
    catch (err)
    {
        dateformat = 'm/d/y';
    }
    if ($(".datepicker").size() > 0) {
        var minDate = $(".datepicker").attr("minDate");
        $(".datepicker").datepicker({
            showOn: "button",
            buttonImage: "/Content/Images/icons/calendar_icon.png",
            buttonImageOnly: true,
            dateFormat: dateformat,
            minDate: minDate
        });
    }

    $(".datepickerFuture").datepicker({
        showOn: "button",
        buttonImage: "/Content/Images/icons/calendar_icon.png",
        buttonImageOnly: true,
        buttonText: "Select date",
        showOtherMonths: true,
        selectOtherMonths: true,
        minDate: new Date()
    });
    //


    $(".datepickerBirthDate").datepicker({
        showOn: "button",
        buttonImage: "/Content/Images/icons/calendar_icon.png",
        buttonImageOnly: true,
        buttonText: "Select date",
        changeMonth: true,
        changeYear: true,
       // showOtherMonths: true,
      //  selectOtherMonths: true,
        yearRange: '1920:' + new Date().getFullYear().toString(),
      //  maxDate: new Date()
       
    });
    
    if ($('#lblErrorMessage').text() != "") {
        showStickMessage($('#lblErrorMessage').text());
        $('#lblErrorMessage').text('');
    }
    if ($('#lblMessage').text() != "") {
        showMessage($('#lblMessage').text());
        $('#lblMessage').text('');
    }

    //if (document.URL.toLowerCase().indexOf('/Golfler/') > -1 )
    AccessRights();



    //Show Dropdown is empty
    $('select').each(function () {
        if ($(this).children().size() == 0) {
            $(this).append('<option value=""><-- Not Available --></option>');
        }
        //        else if ($(this).children().size() == 1 &&
        //                ($(this).eq(0).val() == "" ||
        //                 $(this).eq(0).val() == "0")) {
        //            $(this).children().remove()
        //            $(this).append('<option value=""><-- Not Available --></option>');
        //        }
        //        else {
        //            // Keep track of the selected option.
        //            var selectedValue = $(this).val();

        //            // Sort all the options by text. I could easily sort these by val.
        //            $(this).html($("option", $(this)).sort(function (a, b) {
        //                return a.text == b.text ? 0 : a.text < b.text ? -1 : 1
        //            }));

        //            // Select one option.
        //            $(this).val(selectedValue);
        //        }
    });
});

function fix_select(selector) {
    var i = $(selector).parent().find('div,ul').remove().css('zIndex');
    $(selector).unwrap().removeClass('jqTransformHidden').jqTransSelect();
    $(selector).parent().css('zIndex', i);
    $(selector).parent().css("border-color", "rgb(229, 229, 229)");
}

function removeDuplicates(inputArray) {
    var i;
    var len = inputArray.length;
    var outputArray = [];
    var temp = {};

    for (i = 0; i < len; i++) {
        temp[inputArray[i]] = 0;
    }
    for (i in temp) {
        outputArray.push(i);
    }
    return outputArray;
}

function CheckBoxChecks(id, action) {
    $(id).on('click', function () {
        checkAll(id, action);
    });
    isAllChecked(action, id);
    $('input[action=' + action + ']').on('click', function () {
        isAllChecked(action, id);
    });
}

function checkAll(id, action) {
    var isChecked = $(id).is(':checked');
    $('input[action=' + action + ']').prop("checked", isChecked);
}

function isAllChecked(action, id) {
    // alert("total: " + $('input[action=' + action + ']').length);
    //alert("selected: " + $('input[action=' + action + ']:checked').length);
    var isChecked = ($('input[action=' + action + ']').length == $('input[action=' + action + ']:checked').length);
    $(id).prop("checked", isChecked);
}

function CountryStateSetting(countryfield, statefield, isRequired) {
    if ($('#ID').val() == "0" || $('#ID').size() == 0)
        $('#ddl' + countryfield).val("US");

    var country = $('#ddl' + countryfield).val();

    if (country == 'US') {
        // in case of jqtransform
        $('#ddl' + statefield).removeAttr('disabled');
        $('#ddl' + statefield).removeClass('disable');
        $('#ddl' + statefield).parent().removeClass('d-disable');
        //        if (document.URL.toLowerCase().indexOf('/appstore/') == -1)
        //            $('#ddl' + statefield).show();
        //        else
        $('#ddl' + statefield).parent().show();
        $('#ddl' + statefield).val($('#txt' + statefield).val());
        $('#txt' + statefield).val('');
        $('#txt' + statefield).hide();
        if ($('#ddl' + statefield).val() != "")
            $('#txt' + statefield).val($('#ddl' + statefield).val());
    } else {
        // in case of jqtransform
        $('#ddl' + statefield).attr('disabled', 'disabled');
        $('#ddl' + statefield).addClass('disable');
        $('#ddl' + statefield).parent().addClass('d-disable');
        //        if (document.URL.toLowerCase().indexOf('/appstore/') == -1)
        //            $('#ddl' + statefield).hide();
        //        else
        $('#ddl' + statefield).parent().hide();
        $('#txt' + statefield).show();
    }

    $('#ddl' + countryfield).parent().bind("keypress", function (e) {
        if (e.keyCode == 13) return false;
    });
    $('#ddl' + statefield).parent().bind("keypress", function (e) {
        if (e.keyCode == 13) return false;
    });

    if (isRequired) {
        $('#ddl' + countryfield).rules('add', {
            required: true,
            messages: {
                required: 'Required'
            }
        });
        $('#txt' + statefield).rules('add', {
            required: true,
            maxlength: 50,
            messages: {
                required: 'Required',
                maxlength: 'Must be under 50 characters'
            }
        });
        $('#ddl' + statefield).rules('add', {
            required: true,
            messages: {
                required: 'Required'
            }
        });
    }


    $('#ddl' + countryfield).on('change', function () {
        var country = this.value;
        $('#txt' + statefield).val('');
        if (country == 'US') {
            $('#txt' + statefield).hide();
            $('#ddl' + statefield).val('');
            // in case of jqtransform
            $('#ddl' + statefield).removeAttr('disabled');
            $('#ddl' + statefield).removeClass('disable');
            $('#ddl' + statefield).parent().removeClass('d-disable');
            //            if (document.URL.toLowerCase().indexOf('/appstore/') == -1)
            //                $('#ddl' + statefield).show();
            //            else
            $('#ddl' + statefield).parent().show();
            $('#ddl' + statefield).focus();
            $('#ddl' + statefield).trigger("change");
        } else {
            // in case of jqtransform
            $('#ddl' + statefield).attr('disabled', 'disabled');
            $('#ddl' + statefield).addClass('disable');
            $('#ddl' + statefield).parent().addClass('d-disable');
            //            if (document.URL.toLowerCase().indexOf('/appstore/') == -1)
            //                $('#ddl' + statefield).hide();
            //            else
            $('#ddl' + statefield).parent().hide();
            $('#txt' + statefield).show();

            setTimeout(function () {
                $('#txt' + statefield).focus();
            }, 0);

            $('#txt' + statefield).trigger("blur");
        }
    });
    $('#ddl' + statefield).on('change', function () {
        $('#txt' + statefield).val(this.value);
    });
}

NoAction = 'javascript:return false;';

function AccessRights() {

    try {
        if ($('#AddFlag').size() != 0 && $('#AddFlag').val() != "True") {
            $('input[action="add"]').addClass('unaccess');
        }
        if ($('#DeleteFlag').size() != 0 && $('#DeleteFlag').val() != "True") {
            $('input[action="delete"]').addClass('unaccess');
        }
        if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True") {
            $('input[action="update"]').addClass('unaccess');
        }

        var id = -1;
        if (document.URL.toLowerCase().indexOf('editorg') != -1 || document.URL.toLowerCase().indexOf('orgaddedit') != -1) {
            if ($("#hdEditId").size() > 0)
                id = $("#hdEditId").val();
        } else {
            if ($("#ID").size() > 0)
                id = $("#ID").val();
        }

        if (id != -1) {
            if (id == 0) {
                if ($('#AddFlag').size() != 0 && $('#AddFlag').val() != "True")
                    $('input[action="add"]').addClass('unaccess');
                else
                    $('input[action="add"]').removeClass('unaccess');
            } else {
                if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True")
                    $('input[action="add"]').addClass('unaccess');
                else
                    $('input[action="add"]').removeClass('unaccess');
            }
        }

        //        if (document.URL.toLowerCase().indexOf('editorg') != -1 || document.URL.toLowerCase().indexOf('orgaddedit') != -1) {
        //            //for organization pages
        //            if ($("#hdEditId").val() == 0) {
        //                if ($('#AddFlag').size() != 0 && $('#AddFlag').val() != "True") {
        //                    if ($('#firststep').size() > 0) {
        //                        $('#firststep').addClass('unaccess');
        //                    }
        //                }
        //            }
        //            else {
        //                if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True") {
        //                    if ($('#firststep').size() > 0) {
        //                        $('#firststep').addClass('unaccess');
        //                    }
        //                }
        //            }
        //        }
        //        else {
        //            //for other pages

        //            if ($('#ID').val() == 0) {
        //                if ($('#AddFlag').size() != 0 && $('#AddFlag').val() != "True") {
        //                    if ($('#btnSubmit').size() > 0) {
        //                        $('#btnSubmit').addClass('unaccess').attr('onclick', NoAction);
        //                    }
        //                }
        //            }
        //            else if ($('#UpdateFlag').size() != 0 && $('#UpdateFlag').val() != "True") {

        //                if ($('#btnSubmit').size() > 0) {
        //                    $('#btnSubmit').addClass('unaccess').attr('onclick', NoAction);
        //                }
        //                if ($('#firststep').size() > 0) {
        //                    $('#firststep').addClass('unaccess').attr('onclick', NoAction);
        //                }
        //            }
        //        }

       // if (document.URL.toLowerCase().indexOf('CourseBuilder') != -1) {
            $('.unaccess').attr('onclick', NoAction).on("click", function () {
                showStickMessage("You are not authorized to perform this action.");
                return false;
            });
        //}


    } catch (ex) {
        alert("Access Rights: " + ex.message);
    }
}


function PriceFilter(field) {
    var foo = document.getElementById(field);

    foo.addEventListener('input', function (prev) {
        return function (evt) {
            if (!/^\d{0,9}(?:\.\d{0,9})?$/.test(this.value)) {
                this.value = prev;
                $('[id$=lblMax]').text("Enter valid numeric value for price");
                $('[id$=lblMax]').css("color", "red");
                $('[id$=lblMax]').css("padding-left", "150px");
            } else {
                prev = this.value;
                $('[id$=lblMax]').text("");
            }
        };
    }(foo.value), false);
}

//by varinder
// for required validation
function ValidateTxtReq(txt) {

    if ($.trim($(txt).val()) == "") {
        $(txt).parent().find('.field-validation-error').remove();
        $(txt).addClass("input-validation-error");
        $(txt).parent().append("<span class='field-validation-error'><span>required</span></span>");
        return false;
    } else {
        $(txt).parent().find('.field-validation-error').remove();
        $(txt).removeClass("input-validation-error");
        return true;
    }
}


//by varinder
// for Email validation
function validateEmail(txt) {

    var str = $.trim($(txt).val());

    if (str != "") {

        var patt1 = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
        if (str.match(patt1)) {

            $(txt).parent().find('.field-validation-error').remove();
            return true;
        } else {

            $(txt).parent().find('.field-validation-error').remove();
            $(txt).addClass("input-validation-error");
            $(txt).parent().append("<span class='field-validation-error'><span>Please enter valid email.</span></span>");
            return false;
        }
    } else {
        $(txt).removeClass("input-validation-error");
        $(txt).parent().find('.field-validation-error').remove();
        return true;
    }
}


//by varinder
// for Numeric validation
function validateInt(txt) {

    var str = $.trim($(txt).val());

    var patt1 = /\d+$/;
    if (str.match(patt1)) {
        $(txt).parent().find('.field-validation-error').remove();
        $(txt).removeClass("input-validation-error");
        return true;
    } else {

        $(txt).parent().find('.field-validation-error').remove();
        $(txt).addClass("input-validation-error");
        $(txt).parent().append("<span class='field-validation-error'><span>Please enter valid Port.</span></span>");
        return false;
    }
}


//by varinder
// for Alpha Numeric with spaces
function validateAlphaNumeric(txt) {

    var str = $.trim($(txt).val());

    //var patt1 = ([a-zA-Z0-9 ]+);
    var patt1 = /^[a-zA-Z0-9 ]+$/;
    if (str.match(patt1)) {
        $(txt).parent().find('.field-validation-error').remove();
        $(txt).removeClass("input-validation-error");
        return true;
    } else {

        $(txt).parent().find('.field-validation-error').remove();
        $(txt).addClass("input-validation-error");
        $(txt).parent().append("<span class='field-validation-error'><span>Only alphanumeric and space are allowed.</span></span>");
        return false;
    }
}

function displayLabel(element) {
    if ($(element)[0].tagName == "INPUT") {
        $(element).parent().append("<label style='padding: 11px 0 0; position:absolute'>" + $(element).val() + "</label>");
    } else if ($(element)[0].tagName == "SELECT") {
        var id = $(element).attr("id");
        $(element).parent().append("<label style='padding: 11px 0 0; position:absolute'>" + $("#" + id + " option:selected").text() + "</label>");
    }
    $(element).css("display", "none");
}



$(document).ready(function () {
    //    $("[readonly]").each(function () {
    //        displayLabel(this);
    //    });
    //    $("[disabled]").each(function () {
    //        displayLabel(this);
    //    });


    if ($(".searchfeild").size() > 0) {
        $(".searchfeild").attr("maxlength", "50");
    }

    if ($(".searchfeild_").size() > 0) {
        $(".searchfeild_").attr("maxlength", "50");
    }


    var count = 0;
    $('.ToolTipIcon').each(function () {
        if ($(this).children().size() == 0) {
            var ToolTipText = $(this).attr("message")
            $(this).append("<img src='" + resUrl + "images/info.gif' />");
            $(this).append("<div id='Tooltip" + count + "' style='display: none;'>" + ToolTipText + "</div>");
            $(this).easyTooltip({
                useElement: "Tooltip" + count
            });
            count++;
        }
    });

    AddPlaceHolder(':text[placeholder]');
});



function OnlyImages(element) {
    var val = $(element).val();
    if (!val.match(/(?:gif|jpg|jpeg|png|bmp)$/)) {
        $(element).parent().find('.field-validation-error').remove();
        $(element).parent().append("<span class='field-validation-error'><span>*.png,*.gif,*.jpg,*.jpeg,*.bmp files only.</span></span>");
        $(element).val('');
    } else {
        $(element).parent().find('.field-validation-error').remove();
        $(element).removeClass("input-validation-error");
    }
}



function AddPlaceHolder(selector) {

    if ($.browser.msie != undefined && $.browser.msie == true) {

        if (!$.support.placeholder) {
            $.valHooks.input = {
                get: function (elem) {
                    if ($(elem).attr('placeholder') == elem.value)
                        return "";
                    else
                        return elem.value;
                },
                set: function (elem, value) {
                    elem.value = value;
                }
            };

            //            $(document).delegate(selector, "focus", function () {
            //                if ($(this).attr('placeholder') != '' && $(this).val() == '') {
            //                    $(this).val('').removeclass('hasplaceholder');
            //                    $(this).css("color", "");
            //                }
            //            });

            //            $(document).delegate(selector, "blur", function () {
            //                if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
            //                    $(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
            //                    $(this).css("color", "#A4A3A2");
            //                }
            //            });

            $(document).on("mouseover mouseout", function () {
                $(selector).blur();
            });

            //            $(':text').focus(function () {
            //                if ($(this).attr('placeholder') != '' && $(this).val() == $(this).attr('placeholder')) {
            //                    $(this).val('').removeClass('hasPlaceholder');
            //                    $(".hasPlaceholder").css("color", "#A4A3A2");
            //                }
            //            }).blur(function () {
            //                if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
            //                    $(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
            //                    $(".hasPlaceholder").css("color", "#A4A3A2");
            //                }
            //            });
            //            $(':text').blur();
        }
    }
}

$(function () {
    $('.password-meter').removeAttr('style');
    if (document.URL.toLowerCase().indexOf('/appstore/') != -1) {
        //        debugger;
        //        $('#BannerToUpload').bind('select', function () {
        //            debugger;
        //            alert($('#BannerToUpload').val());
        //        });
        $uniformed = $("input[type='file']").not(".skipThese");
        $uniformed.uniform();


        $("input[fileType='Image']").each(function () {
            $(this).change(function () {
                var val = $(this).val();
                if (!val.match(/(?:gif|jpg|png|bmp)$/)) {

                    $(this).parent().parent().find('.field-validation-error').remove();
                    //$("#file").addClass("input-validation-error");
                    $(this).parent().parent().append("<span class='field-validation-error'><span>*.png,*.gif,*.jpg,*.jpeg,*.bmp files only.</span></span>");
                    $(this).val('');
                    $(this).parent().find(".filename").html('No file selected');

                } else {
                    $(this).parent().parent().find('.field-validation-error').remove();
                    $(this).removeClass("input-validation-error");
                }

            });
        });

        if ($('#ltrBreadCrumb').size() > 0) {
            //$("#ltrBreadCrumb").html(($("#ltrBreadCrumb").html().replace('&gt;', "<span class='breadCrumbArrow'>></span>")));
            $("#ltrBreadCrumb").html(($("#ltrBreadCrumb").html().replace(/\&gt;/g, "<span class='breadCrumbArrow'>></span>")));
        }
    }
});


$(document).ready(function () {
    $('span:contains("*")').remove();
});


$(document).ready(function () {
    //$("#page-wrap div").attr("style", "width:100% !important;");

    $("#page-wrap div").css("width", "100%", "important;");
    $("#page-wrap table").css("width", "100%", "important;");

    $("#alertmod_list").hide();



    if (document.URL.toLowerCase().indexOf('/appstore/') != -1) {

        if ((!(document.URL.toLowerCase().indexOf('organization/mdmsettings') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/mdmdeviceinventory') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/oneclickinstall') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/assignvoucher') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/assignbudget') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/managebudget') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/GetCaseInfo') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/MDMReports') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('organization/userlist') != -1)) &&
            //(!(document.URL.toLowerCase().indexOf('organization/enduserlist') != -1)) &&
            (!(document.URL.toLowerCase().indexOf('advertiser/appdirectorysearch') != -1))) {
            bindCustomDropdown('body');
        }
    }

    $('input[type="text"]').not('[data-val-required]').live().each(function () {
        $(this).css('background-image', 'none');
    });

    $('textarea').not('[data-val-required]').live().each(function () {
        $(this).css("border-color", " #E5E5E5 #E5E5E5 #E5E5E5 #E5E5E5");
    });

    ////replacing # from anchor tags
    $('a[href="#"]').each(function () {
        this.href = 'javascript:';
    });
});

function bindCustomDropdown(select) {
    try {
        $(select).jqTransform({
            imgPath: 'Content/Images/'
        });

        $(select + " select").attrchange({
            trackValues: true,
            callback: function (event) {
                if ($(this).hasClass('input-validation-error')) {
                    //$(this).next('.btn-group').addClass('input-validation-error');
                    $(this).parent('.jqTransformSelectWrapper').addClass('input-validation-error');
                }
            }
        });

        $(select + " select").change(function () {
            if ($(this).val() != "" || $(this).not('[data-val-required]').size() > 0) {
                $(this).parent('.jqTransformSelectWrapper').removeClass('input-validation-error');
                $(this).parent().parent().find(".field-validation-error span").hide();
            } else {
                $(this).parent('.jqTransformSelectWrapper').addClass('input-validation-error');
                $(this).parent().parent().find(".field-validation-error span").show();
            }
        });

        $(select + " select").not('[data-val-required]').live().each(function () {
            $(this).parent().css("border-color", " #E5E5E5 #E5E5E5 #E5E5E5 #E5E5E5");
        });

        //        $(select + " select").live().each(function () {
        //            if (!($.trim($(this).attr('data-val-required')) == '') || !$(this).hasClass('red-border'))
        //                $(this).parent().css("border-color", " #E5E5E5 #E5E5E5 #E5E5E5 #E5E5E5");
        //        });

        $("#jqTransformSelectWrapper select").css("width", "100%", "important;");


        $(select + ' .jqTransformSelectWrapper a').each(function () {
            while ($(this).html().indexOf(' ') != -1) {
                $(this).html($(this).html().replace(' ', '&nbsp;'));
            }
        });

        $(select + ' .jqTransformSelectWrapper ul').css('overflow', '');
        $(select + ' select[disabled="disabled"]').parent('div').addClass('d-disable');
        $(select + ' select[disabled="disabled"]').addClass('disable');
    } catch (Error) { }
}

var overlaypop = $('<div class="overlayFrontEnd"></div>');

function showPopup(id, data) {
    if (data != null)
        $('#' + id).append(data);
    overlaypop.show();
    overlaypop.appendTo(document.body);
    $('#' + id).show();
}

function closePopup(id, emptyDiv) {
    $('#' + id).hide();
    if (emptyDiv)
        $('#' + id).html('');
    overlaypop.hide();
    overlaypop.appendTo(document.body).remove();
    return false;
}

(function ($) {
    $.QueryString = (function (a) {
        if (a == "") return {};
        var b = {};
        for (var i = 0; i < a.length; ++i) {
            var p = a[i].split('=');
            if (p.length != 2) continue;
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
        }
        return b;
    })(window.location.search.substr(1).split('&'))
})(jQuery);

function sessionExpires(data) {
    if (data.message == 'sessionexpire') {
        alert(resSessionExpire);
        window.location = data.url;
        throw new Error(resSessionExpire);
    }
}


function NonAccessMsg() {
    showStickMessage('You are unautherized to perform this action.');
    return false;
}
