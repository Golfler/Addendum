/***********************************************************************************************
                             Script taken from some third party site
				 The script has been modified by Steve Curry to meet the requirements.
************************************************************************************************
To swap between stylesheets:

	changeStyle();                           //switches off all stylesheets that have title attributes
	changeStyle('Default');                  //switches off all stylesheets that have title attributes that do not match 'Default'
	changeStyle('High Contrast');            //switches off all stylesheets that have title attributes that do not match 'High Contrast'
	changeStyle('Big Font');                 //switches off all stylesheets that have title attributes that do not match 'Big Font'
	changeStyle('High Contrast','Big Font'); //switches off all stylesheets that have title attributes that do not match 'High Contrast' or 'Big Font'

	Opera 7+ and Mozilla also allow users to switch between stylesheets using the view menu (only one at a time though ...)

To make the script remember the user's choice of stylesheets, for example to use on more than one page or if they reload
- includes stylesheets chosen using the view menu in Gecko - it will only attempt to store a cookie if they actually
changed something:

	In these examples, I call the cookie used to store the choice 'styleTestStore'. You could use any name you like.

	To remember only until the browser window is closed:

		<body onload="useStyleAgain('styleTestStore');" onunload="rememberStyle('styleTestStore');">

	To remember for 10 days (for example):

		<body onload="useStyleAgain('styleTestStore');" onunload="rememberStyle('styleTestStore',10);">_____________________________________________________________________________________________*/

function getAllSheets() {
	if( !window.ScriptEngine && navigator.__ice_version ) { return document.styleSheets; }
	if( document.getElementsByTagName ) { var Lt = document.getElementsByTagName('link'), St = document.getElementsByTagName('style');
	} else if( document.styleSheets && document.all ) { var Lt = document.all.tags('LINK'), St = document.all.tags('STYLE');
	} else { return []; } for( var x = 0, os = []; Lt[x]; x++ ) {
		var rel = Lt[x].rel ? Lt[x].rel : Lt[x].getAttribute ? Lt[x].getAttribute('rel') : '';
		if( typeof( rel ) == 'string' && rel.toLowerCase().indexOf('style') + 1 ) { os[os.length] = Lt[x]; }
	} for( var x = 0; St[x]; x++ ) { os[os.length] = St[x]; }  return os;
}
function changeStyle() {
    
	window.userHasChosen = window.MWJss;
	for (var x = 0, ss = getAllSheets(); ss[x]; x++) {	  
		if( ss[x].title ) { ss[x].disabled = true; }
		for( var y = 0; y < arguments.length; y++ ) { if( ss[x].title == arguments[y] ) { ss[x].disabled = false; } }
} }
function rememberStyle( cookieName, cookieLife ) {
	for( var viewUsed = false, ss = getAllSheets(), x = 0; window.MWJss && MWJss[x] && ss[x]; x++ ) { if( ss[x].disabled != MWJss[x] ) { viewUsed = true; break; } }
	if( !window.userHasChosen && !viewUsed ) { return; }
	for( var x = 0, outLine = '', doneYet = []; ss[x]; x++ ) {
		if( ss[x].title && ss[x].disabled == false && !doneYet[ss[x].title] ) { doneYet[ss[x].title] = true; outLine += ( outLine ? ' MWJ ' : '' ) + escape( ss[x].title ); } }
	if( ss.length ) { document.cookie = escape( cookieName ) + '=' + escape( outLine ) + ( cookieLife ? ';expires=' + new Date( ( new Date() ).getTime() + ( cookieLife * 86400000 ) ).toGMTString() : '' ) + ';path=/'; }
}
function useStyleAgain( cookieName ) {
	for( var x = 0; x < document.cookie.split( "; " ).length; x++ ) {
		var oneCookie = document.cookie.split( "; " )[x].split( "=" );
		if( oneCookie[0] == escape( cookieName ) ) {
			var styleStrings = unescape( oneCookie[1] ).split( " MWJ " );
			for( var y = 0, funcStr = ''; styleStrings[y]; y++ ) { funcStr += ( y ? ',' : '' ) + 'unescape( styleStrings[' + y + '] )'; }
			eval( 'changeStyle(' + funcStr + ');' ); break;
	} } window.MWJss = []; for( var ss = getAllSheets(), x = 0; ss[x]; x++ ) { MWJss[x] = ss[x].disabled; }
}