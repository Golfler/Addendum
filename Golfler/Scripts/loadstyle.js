// JScript File
//JS FILE TO LOAD STYLE SHEETS ACCORDING TO THEME
function loadStyle(optionValue)
{
	mycookies = document.cookie;
	if ((mycookies.search("global-snowy") > 0))
	{
		self.location.reload();
	}
	if (optionValue == "default")
	{
		changeStyle('global');
	}
	else if (optionValue == "blue")
	{
		changeStyle('global-blue');
	}
	else if (optionValue == "purple")
	{
		changeStyle('global-purple');
	}
	else if (optionValue == "grey")
	{
		changeStyle('global-grey');
	}
	else if (optionValue == "mosaic")
	{
		changeStyle('global-mosaic');
	}
	else if (optionValue == "transparent")
	{
		changeStyle('global-transparent');
	}
	else if (optionValue == "snowy")
	{
		changeStyle('global-snowy');
		self.location.reload();		
	}
	else
	{
		changeStyle('global-green');
	}
}
changeStyle('global');
useStyleAgain('styleTestStore');