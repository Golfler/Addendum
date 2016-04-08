// JavaScript Document
// SEGNANT: Function defined to add site bookmark. This funciton will add the currently opened page as a bookmark
//Programmer: Steve Curry
function bookmark()
{
	netscape="Netscape User's hit CTRL+D to add a bookmark to this site.";
	url=window.location.href; // Getting the url of the current page.
	docTitle = document.title; // Getting the title of the current page.
	if (navigator.appName=='Microsoft Internet Explorer')
	{
		window.external.AddFavorite(url,docTitle);
	}
	else if (navigator.appName=='Netscape')
	{
		alert(netscape);
	}
}