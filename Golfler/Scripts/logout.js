function LogOut()
	{
		ht = document.getElementsByTagName("html");
		ht[0].style.filter = "progid:DXImageTransform.Microsoft.BasicImage(grayscale=1)";
		if (confirm('Are you sure you want to logout?'))
		{
			return true;
		}
		else
		{
			ht[0].style.filter = "";
			return false;
		}
	}