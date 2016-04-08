var OrganizationId;
//var root = "../../";

function LoadTempTop(module) {

    OrganizationId = $("#hdOrgID").val();
    var layoutcatid = $('#layouthdCat').val();
    var layoutdepid = $('#layouthdDept').val();

    if (document.URL.toLocaleLowerCase().indexOf('preview=true') == -1) {
        $("#divProgress").show();
    }


    $.ajax({
        type: 'POST',
        url: '/Share/Header',
        data: { OrgId: OrganizationId, catid: layoutcatid, depid: layoutdepid },
        success: function (data) {
            $('#headercontent').replaceWith(data);
            AdSetting();
        }
    });

    //    if (module == 'CategoryList' || module == 'Getapplist' || module == 'GetappsDetail' || module == 'getappsetlist' ||
    //        module == 'Home' || module == 'ContactUs') {
    $.ajax({
        type: 'POST',
        url: '/Share/CloudTag',
        data: { orgId: OrganizationId },
        success: function (data) {
            $('#cloudTagcontent').html(data);
            $('#cloudTag').removeAttr('style');
        }
    });
    //    }
    //    else {
    //        $('#cloudTagcontent').remove();
    //    }
}

function LoadTempBottom(module) {
    if (module == 'Home') {

        $.ajax({
            type: 'POST',
            url: '/Share/FeaturedApp',
            data: {},
            success: function (data) {
                $('#featureappcontent').html(data);
                $('a[href="#"]').each(function () {
                    this.href = 'javascript:';
                });
            }
        });
    }
    else {
        $('.banner').remove();
        //$('.banner_bg').addClass('contact_us');
        $('body').addClass('innerpages');
    }


    if (module == 'App_Store') {

        $.ajax({
            type: 'POST',
            url: '/Share/LatestUpdated',
            data: {},
            success: function (data) {

                $('#featureappcontent').html(data).append('<div class="clear" style="margin-bottom:40px" ></div>');
                $('a[href="#"]').each(function () {
                    this.href = 'javascript:';
                });
                AfterPageLoad();

                $.ajax({
                    type: 'POST',
                    url: '/Share/FeaturedApp',
                    data: {},
                    success: function (data) {

                        $('#featureappcontent').append(data);
                        $('a[href="#"]').each(function () {
                            this.href = 'javascript:';
                        });
                        $("span:contains('Recent App Reviews')").html('Recommended for You');
                        $("a:contains('More Featured Apps')").html('See More');
                    }
                });
            }
        });

        


    }



    $.ajax({
        type: 'POST',
        url: '/Share/Footer',
        data: { OrgId: OrganizationId },
        success: function (data) {
            $('#footermenucontent').html(data);
            $('a[href="#"]').each(function () {
                this.href = 'javascript:';
            });
            $("#divProgress").hide();
        }
    });

    $('.verticalNav').prependTo('#categorycontent');
    if ($('.verticalNav ul li').size() == 0)
        $('.verticalNav').css('display', 'none');
}

function LoadInnerTemplate(module) {
    //root = "../../";
    if (module == 'Home') {
        $.ajax({
            type: 'POST',
            url: '/Share/Banner',
            data: {},
            success: function (data) {
                //$('#bannercontent').html(data);
                $('#main_banner').css('background', 'url(' + $(data).attr('src') + ') top center');
            }
        });
    }

    LoadTempTop(module);

    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { orgId: OrganizationId },
        success: function (data) {

            $('#' + module + 'content').html(data);
            AfterPageLoad();
        }
    });

    $('body').attr('id', module);

    LoadTempBottom(module);
}

function LoadAppStoreTemplate(module) {
    //root = "../../";
    if (module == 'Home') {
        $.ajax({
            type: 'POST',
            url: '/Share/Banner',
            data: {},
            success: function (data) {
                //$('#bannercontent').html(data);
                $('#main_banner').css('background', 'url(' + $(data).attr('src') + ') top center');
            }
        });
    }

    LoadTempTop(module);



    $('body').attr('id', module);

    LoadTempBottom(module);
}

function AdSetting() {
    if ($('#TAd').size() > 0) {
        $('#topadcontent').replaceWith($('#TAd'));
        $('#TAd').css("display", "");
    }
    else {
        $('#topadcontent').parent().remove();
    }

    if ($('#BAd').size() > 0) {
        $('#bottomadcontent').replaceWith($('#BAd'));
        $('#BAd').css("display", "");
    }
    else {
        $('#bottomadcontent').parent().remove();
    }

    if ($('#LAd').size() > 0) {
        $('#leftadcontent').replaceWith($('#LAd'));
        $('#LAd').css("display", "");
    }
    else {
        $('#leftadcontent').parent().remove();
    }

    if ($('#RAd').size() > 0) {
        $('#rightadcontent').replaceWith($('#RAd'));
        $('#RAd').css("display", "");
    }
    else
        $('#rightadcontent').parent().remove();

    if ($('#RAd').size() == 0 && $('#LAd').size() == 0) {
        $('.inner_content').addClass('without_add');
    }
    else if ($('#LAd').size() == 0) {
        $('.inner_content').addClass('without_lft_add');
    }
    else if ($('#RAd').size() == 0) {
        $('.inner_content').addClass('without_rt_add');
    }
}

function AfterPageLoad() {
  //  alert("after page load");
    ReplaceContent('breadcrumb');
    ReplaceContent('bannerheader');

    if ($('#breadcrumb').size() != 0) {
        if ($('#categoryList li').size() == 0)
            $('#categorylink').remove();
        else {
            $('#breadcrumb').parent().parent().prepend($('#categorylink'));
           // $("#categorylink").click(function () { $("#categoryList").toggle(); });
            $(document).delegate('#categorylink', 'hover', function () {
                //$('#categorylink').click();
                $("#categoryList").toggle();
                if ($('#see_img1').size() > 0) {
                    if ($('#see_img1').is(":visible")) {
                        $('#see_img2').show();
                        $('#see_img1').hide();
                    }
                    else {
                        $('#see_img1').show();
                        $('#see_img2').hide();
                    }
                }
            });
            if ($('#categoryList #sidesubcatlist').size() > 0)
                $('.all_categories').html($('.all_categories').html().replace('See All Categories', 'See Sub Categories'));
        }

    }
    else {
        $("#categorylink").remove();
    }
}

function LoadInnerTemplatePage(module, name) {//Static Page
    //root = "../../../";
    LoadTempTop(module);

    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { orgId: OrganizationId, name: name },
        success: function (data) {
            $('#' + module).html(data);
            AfterPageLoad();
        }
    });
    $('body').attr('id', name);

    LoadTempBottom(module);
}

function LoadInnerTemplateForAppListing(module, categoryId, _depid, keyword, tag, IsFeatured) {
    LoadTempTop(module);
    var data1;
    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { orgId: OrganizationId, catid: categoryId, depid: _depid, keyword: keyword, tag: tag, IsFeatured: IsFeatured },
        success: function (data) {
            $('#' + module + 'content').html(data);
            AfterPageLoad();
        }
    });
    $('body').attr('id', module);
    LoadTempBottom(module);
}

function LoadInnerTemplateForAppDetails(module, appid, catid) {
 //   debugger;
 //   alert("loading");
    LoadTempTop(module);
    var data1;
    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { id: appid, catid: catid },
        success: function (data) {
//            debugger;
            $('#' + module + 'content').html(data);
            AfterPageLoad();
        }
    });
    $('body').attr('id', module);
    LoadTempBottom(module);
}

function LoadInnerTemplateForAdvanceSearch(module, page, rows, categoryId, platform, rating, keyword, depid) {
    LoadTempTop(module);
    var data1;
    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { page: page, rows: rows, orgId: OrganizationId, catid: categoryId, platform: platform, rating: rating,
            keyword: keyword, depid: depid
        },
        success: function (data) {
            $('#' + module + 'content').html(data);
            AfterPageLoad();
        }
    });
    $('body').attr('id', module);
    LoadTempBottom(module);
}

function LoadInnerTemplateForAppSetList(module) {

    LoadTempTop(module);
    $.ajax({
        type: 'POST',
        url: '/Share/' + module,
        data: { orgId: OrganizationId },
        success: function (data) {

            $('#div' + module).html(data);
        }
    });
    LoadTempBottom();
}

function ReplaceContent(part) {
    $('#' + part + 'content').replaceWith($('#' + part));
}