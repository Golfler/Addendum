// JScript File
//JS FILE TO SELECT THEMES
    mycookies = document.cookie;
	if (mycookies.search("global-green") > 0)
	{
	    if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "green";
	}
	else if ((mycookies.search("global-blue") > 0))
	{
	    if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "blue";
	}
	else if ((mycookies.search("global-purple") > 0))
	{
	    if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "purple";
	}
	else if ((mycookies.search("global-grey") > 0))
	{
		if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "grey";
	}
	else if ((mycookies.search("global-mosaic") > 0))
	{
		if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "mosaic";
	}
	else if ((mycookies.search("global-snowy") > 0))
	{
		if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "snowy";
	}
	else if ((mycookies.search("global-transparent") > 0))
	{
		if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "transparent";
	}
	else
	{
	    if(document.getElementById("selTheme"))
		    document.getElementById("selTheme").value = "default";
	}
											
    <!--
    function MM_reloadPage(init)
    { 
       //reloads the window if Nav4 resized
      if (init==true) with (navigator) 
      {
        if ((appName=="Netscape")&&(parseInt(appVersion)==4))
        {
            document.MM_pgW=innerWidth; document.MM_pgH=innerHeight; onresize=MM_reloadPage; 
         }
      }
      else if (innerWidth!=document.MM_pgW || innerHeight!=document.MM_pgH) location.reload();
    }
    MM_reloadPage(true);
    //-->