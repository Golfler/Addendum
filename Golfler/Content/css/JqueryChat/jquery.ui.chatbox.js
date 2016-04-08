/*
 * Copyright 2010, Wen Pu (dexterpu at gmail dot com)
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://jquery.org/license
 *
 * Check out http://www.cs.illinois.edu/homes/wenpu1/chatbox.html for document
 *
 * Depends on jquery.ui.core, jquery.ui.widiget, jquery.ui.effect
 *
 * Also uses some styles for jquery.ui.dialog
 *
 */

// TODO: implement destroy()

var isClose = false; var isCloseonToggle = "";
(function ($) {


    $.widget("ui.chatbox", {
        options: {
            id: null, //id for the DOM element
            title: null, // title of the chatbox
            user: null, // can be anything associated with this chatbox
            hidden: false,
            offset: 0, // relative to right edge of the browser window
            width: 300, // width of the chatbox
            messageSent: function (id, user, msg) {
                // override this
                var d =  new Date();
              
                this.boxManager.addMsg(user.last_name + " ( " + getISODateTime(d) + " ) ", msg, 1);
                // debugger;
                //alert(id);
                SendMessage(msg, id);
                //  setTimeout(function () {
                // Do something after 1 Minute=60*1000 
                //  GetMessage(id, 1);
                // call service to get msg
                //}, 1000)
            },
            boxClosed: function (id) {

            }, // called when the close icon is clicked
            boxManager: {
                // thanks to the widget factory facility
                // similar to http://alexsexton.com/?p=51
                init: function (elem) {
                    this.elem = elem;
                },
                addMsg: function (peer, msg, issend) {
                    var self = this;

                    var box = self.elem.uiChatboxLog;
                    var e = document.createElement('div');
                    box.append(e);
                    $(e).hide();

                    var systemMessage = false;

                    if (peer) {
                        var peerName = document.createElement("b");
                        $(peerName).text(peer + ": ");
                        e.appendChild(peerName);
                    }
                    else {
                        systemMessage = true;
                    }

                    var msgElement = document.createElement(
                        systemMessage ? "i" : "span");
                    $(msgElement).text(msg);
                    e.appendChild(msgElement);

                    //EXTRA
                    if (issend == "1") {
                        $(e).addClass("sent-message");
                    }
                    else {
                        $(e).addClass("received-message");
                    }
                    //
                    $(e).addClass("ui-chatbox-msg");

                    $(e).css("maxWidth", $(box).width());
                    $(e).fadeIn();
                    self._scrollToBottom();

                    if (!self.elem.uiChatboxTitlebar.hasClass("ui-state-focus")
                        && !self.highlightLock) {
                        self.highlightLock = true;
                        self.highlightBox();
                    }

                },
                highlightBox: function () {
                    var self = this;
                      self.elem.uiChatboxTitlebar.effect("highlight", {}, 300);
                    self.elem.uiChatbox.effect("bounce", { times:2 }, 300, function () {
                       self.highlightLock = false;
                       self._scrollToBottom();
                    });
                },
                toggleBox: function () {
                    this.elem.uiChatbox.toggle();
                },
                _scrollToBottom: function () {
                    var box = this.elem.uiChatboxLog;
                    box.scrollTop(box.get(0).scrollHeight);
                }
            }
        },
        toggleContent: function (event) {
            this.uiChatboxContent.toggle();
          //  debugger;
            if (this.uiChatboxContent.is(":visible")) {
                this.uiChatboxInputBox.focus();
               // this.uiChatboxTitlebar.removeClass('ui-chatbox-min');
            }


            else {
              //  isClose = true;
              //  debugger;
                //if (isCloseonToggle == true && isClose == true)
                //{ }
                //else
                //{
                    // if (isClose == false) {
             //     this.uiChatboTitlebar.addClass('ui-chatbox-min');
                //}
            }
        },
        widget: function () {
            return this.uiChatbox
        },
        _create: function () {
           // alert("hi");
            var self = this,
            options = self.options,
            title = options.title || "No Title",
            // chatbox
            uiChatbox = (self.uiChatbox = $('<div></div>'))
                .appendTo(document.body)
                .addClass('ui-widget ' +
                          'ui-corner-top ' +
                          'ui-chatbox'
                         )
                .attr('outline', 0)
                .attr('name', self.options.id)
                .attr('status', '1')
                .focusin(function () {
                    // ui-state-highlight is not really helpful here
                    //self.uiChatbox.removeClass('ui-state-highlight');
                    self.uiChatboxTitlebar.addClass('ui-state-focus');
                })
                .focusout(function () {
                    self.uiChatboxTitlebar.removeClass('ui-state-focus');
                }),
            // titlebar
            uiChatboxTitlebar = (self.uiChatboxTitlebar = $('<div></div>'))
                .addClass('ui-widget-header ' +
                          'ui-corner-top ' +
                          'ui-chatbox-titlebar ' +
                          'ui-dialog-header' // take advantage of dialog header style
                         )
                .click(function (event) {
                    self.toggleContent(event);
                   // alert("ccc");
                    //if (isCloseonToggle == self.options.id)
                    //{
                    //    if (this.uiChatboxTitlebar.hasClass("ui-chatbox-min"))
                    //    {
                    //        this.uiChatboxTitlebar.removeClass('ui-chatbox-min');
                    //    }
                    //    else {
                    //        this.uiChatboxTitlebar.addClass('ui-chatbox-min');
                    //    }
                    //}
                })
                .appendTo(uiChatbox),
            uiChatboxTitle = (self.uiChatboxTitle = $('<span></span>'))
                .html(title)
                .appendTo(uiChatboxTitlebar),
            uiChatboxTitlebarClose = (self.uiChatboxTitlebarClose = $('<a href="#" onclick="funclose();"></a>'))
                .addClass('ui-corner-all ' +
                          'ui-chatbox-icon '
                         )
                .attr('role', 'button')
                .hover(function () { uiChatboxTitlebarClose.addClass('ui-state-hover'); },
                       function () { uiChatboxTitlebarClose.removeClass('ui-state-hover'); })
                .click(function (event) {
                    uiChatbox.hide();
                    //  alert("close only");
                    self.options.boxClosed(self.options.id);
                   
                    //

                    //  debugger;
                    //var findelement = document.getElementsByName(self.options.id);
                    //// alert($(findelement).html());
                    //findelement.removeClass("ui-chatbox-min");
                 
                    //isCloseonToggle = self.options.id;
                    //// debugger;
                    //var NumberOfChatWindows = $(".ui-chatbox").size() - 1;
                    //var width = NumberOfChatWindows * 412;
                    //var widthinPx = width + "px";
                    //$("#divChatParent").css("width", widthinPx);
                    ////

              //      this.parent.attr('closestatus', '0');
                    return false;
                })
                .appendTo(uiChatboxTitlebar),
            uiChatboxTitlebarCloseText = $('<span></span>')
                .addClass('ui-icon ' +
                          'ui-icon-close')
                .text('close')
                .appendTo(uiChatboxTitlebarClose),
            uiChatboxTitlebarMinimize = (self.uiChatboxTitlebarMinimize = $('<a href="#"  onclick="funMinimise();"></a>'))
                .addClass('ui-corner-all ' +
                          'ui-chatbox-icon'
                         )
                .attr('role', 'button')
                .hover(function () { uiChatboxTitlebarMinimize.addClass('ui-state-hover'); },
                       function () { uiChatboxTitlebarMinimize.removeClass('ui-state-hover'); })
                .click(function (event) {
                //    alert("alert");
                    var GolferId = self.options.id.replace("chat_", "");
                    $("#img_" + GolferId).attr("alt", "No New Msgs");
                    $("#img_" + GolferId).attr("src", "../../images/no-new-msg.png");

                    self.toggleContent(event);


                    return false;
                })
                .appendTo(uiChatboxTitlebar),
            uiChatboxTitlebarMinimizeText = $('<span></span>')
                .addClass('ui-icon ' +
                          'ui-icon-minus')
                .text('minimize')
                .appendTo(uiChatboxTitlebarMinimize),
            //    ///my code start
            //      uiChatboxTitlebarRefresh = (self.uiChatboxTitlebarRefresh = $('<a href="#"></a>'))
            //    .addClass('ui-corner-all ' +
            //              'ui-chatbox-icon'
            //             )
            //    .attr('role', 'button')
            //    .hover(function () { uiChatboxTitlebarRefresh.addClass('ui-state-hover'); },
            //           function () { uiChatboxTitlebarRefresh.removeClass('ui-state-hover'); })
            //    .click(function (event) {
            //        //alert("refresh");
            //        GetMessage($("#hdnGolferId").val(), 1);
            //        return false;
            //    })
            //    .appendTo(uiChatboxTitlebar),
            //uiChatboxTitlebarRefreshText = $('<span></span>')
            //    .addClass('ui-icon ' +
            //              'ui-icon-refresh')
            //    .text('refresh')
            //    .appendTo(uiChatboxTitlebarRefresh),
            //    ////my code end
            // content
            uiChatboxContent = (self.uiChatboxContent = $('<div></div>'))
                .addClass('ui-widget-content ' +
                          'ui-chatbox-content '
                         )
                .appendTo(uiChatbox),
            uiChatboxLog = (self.uiChatboxLog = self.element)
                .addClass('ui-widget-content ' +
                          'ui-chatbox-log'
                         )
                .appendTo(uiChatboxContent),
            uiChatboxInput = (self.uiChatboxInput = $('<div></div>'))
                .addClass('ui-widget-content ' +
                          'ui-chatbox-input'
                         )
                .click(function (event) {
                   //  alert("1");
                    var GolferId = self.options.id.replace("chat_", "");
                    $("#img_" + GolferId).attr("alt", "No New Msgs");
                    $("#img_" + GolferId).attr("src", "../../images/no-new-msg.png");
                   
                })
                .appendTo(uiChatboxContent),
            uiChatboxInputBox = (self.uiChatboxInputBox = $('<textarea></textarea>'))
                .addClass('ui-widget-content ' +
                          'ui-chatbox-input-box ' +
                          'ui-corner-all'
                         )
                .appendTo(uiChatboxInput)
                .keydown(function (event) {
                    if (event.keyCode && event.keyCode == $.ui.keyCode.ENTER) {
                        msg = $.trim($(this).val());
                        if (msg.length > 0) {

                            self.options.messageSent(self.options.id, self.options.user, msg);
                        }
                        $(this).val('');
                        return false;
                    }
                })
                .focusin(function () {
                    uiChatboxInputBox.addClass('ui-chatbox-input-focus');
                    var box = $(this).parent().prev();
                    box.scrollTop(box.get(0).scrollHeight);
                })
                .focusout(function () {
                    uiChatboxInputBox.removeClass('ui-chatbox-input-focus');
                });

            // disable selection
            uiChatboxTitlebar.find('*').add(uiChatboxTitlebar).disableSelection();

            // switch focus to input box when whatever clicked
            uiChatboxContent.children().click(function () {
                // click on any children, set focus on input box
                self.uiChatboxInputBox.focus();
            });

            self._setWidth(self.options.width);
            self._position(self.options.offset);

            self.options.boxManager.init(self);

            if (!self.options.hidden) {
                uiChatbox.show();
            }
            //
            uiChatbox.draggable();
            $("#main_panel").append(uiChatbox);
            $("html, body").animate({ scrollTop: $(document).height() }, 1000);
            // debugger;
            var NumberOfChatWindows = $(".ui-chatbox").size();
            var width = NumberOfChatWindows * 412;
            var widthinPx = width + "px";
         //   $("#divChatParent").css("width", widthinPx);
            //
        },
        _setOption: function (option, value) {
            if (value != null) {
                switch (option) {
                    case "hidden":
                        if (value) {
                            this.uiChatbox.hide();
                          //  this.uiChatbox.attr('closestatus', '0');
                        }
                        else {
                            this.uiChatbox.show();
                        //    this.uiChatbox.attr('closestatus', '1');
                        }
                        break;
                    case "offset":
                       // this._position(value);
                        break;
                    case "width":
                       // this._setWidth(value);
                        break;
                }
            }
            $.Widget.prototype._setOption.apply(this, arguments);
        },
        _setWidth: function (width) {
            this.uiChatboxTitlebar.width(width + "px");
            this.uiChatboxLog.width(width + "px");
            this.uiChatboxInput.css("maxWidth", width + "px");
            // padding:2, boarder:2, margin:5
            this.uiChatboxInputBox.css("width", (width - 18) + "px");
        },
        _position: function (offset) {
            
        //    this.uiChatbox.css("right", offset);
        }
    });
}(jQuery));


function getISODateTime(d) {
    // padding function
    var s = function (a, b) { return (1e15 + a + "").slice(-b) };

    // default date parameter
    if (typeof d === 'undefined') {
        d = new Date();
    };

    // return ISO datetime
    return (
      //  d.getUTCMonth() + 1) + '/' +
         d.getMonth() + 1) + '/' +
       d.getDate() + '/' +
      //  d.getUTCFullYear()
    d.getFullYear()
        + ' ' + formatAMPM(d)
      //  s(d.getHours(), 2) + ':' +
       // s(d.getMinutes(), 2) + ':' +
      //  s(d.getSeconds(), 2);
    }
function formatAMPM(date) {

    var hours = date.getHours();//date.getUTCHours();
    var minutes = date.getMinutes(); //date.getUTCMinutes();
    var seconds = date.getSeconds(); //date.getUTCSeconds();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    seconds = seconds < 10 ? '0' + seconds : seconds;
    var strTime = hours + ':' + minutes + ':' + seconds + ' ' + ampm;
    return strTime;
}
function funclose()
{
    // alert("closing");

}
function funMinimise() {
 //   alert("min");
}