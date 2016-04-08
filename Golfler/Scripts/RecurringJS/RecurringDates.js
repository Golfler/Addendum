
var RecurringDates = [];
var weekDaysSelected = [];
var checkstatus = "close";
var RecDateDescription = "";


function generate_recurrence() {
    // alert("start");
    var sDate = document.getElementById("startDate").value;
    var sMonth = document.getElementById("startMonth").value;
    var sYear = document.getElementById("startYear").value;

    var eDate = document.getElementById("endDate").value;
    var eMonth = document.getElementById("endMonth").value;
    var eYear = document.getElementById("endYear").value;

    var startFullDate = sMonth + "/" + sDate + "/" + sYear;
    var endFullDate = eMonth + "/" + eDate + "/" + eYear;

    document.getElementById("start").value = startFullDate;
    document.getElementById("until").value = endFullDate;

    // $("#divProgress").show();
    RecurringDates = [];
    weekDaysSelected = [];
    testvar = "";
    //alert($("#Title").val());
    var pattern = {};
    $('output').value = "";
    // gather pattern
    ['start', 'every', 'unit', 'end_condition', 'until', 'rfor', 'nth', 'occurrence_of'].each(function (k) {
        pattern[k] = $(k).value;
    });


    // gather selected days
    pattern.days = $$('input.week_days').collect(function (d) {
        if (d.checked) {

            weekDaysSelected.push(d.value);
            return d.value;
        }
        return null;
    }).compact();

    try {
        var r = new Recurrence(pattern);
        var dates = r.generate((this.value == '') ? undefined : this.value);
    } catch (e) {
        $('output').value = e.message;
        alert(e.message);
        return;
    }

    // $('output').value = "long:\n" + r.describe() + "\n\n";

    // compact description. next version.
    // $('output').value += "short:\n" + r.describe(true) + "\n\n";

    var strDesc = r.describe();
    strDesc = strDesc.replace("/", "_");
    strDesc = strDesc.replace("/", "_");
    strDesc = strDesc.replace("/", "_");
    strDesc = strDesc.replace("/", "_");
    RecDateDescription = strDesc;
    // alert(RecDateDescription);

    $('output').value += dates.collect(function (d) {
        if (document.getElementById('unit').value == 'w') {
            if (weekDaysSelected.length > 0) {

                var wd = d.toString('ddd');
                var numberofday = getNumberOfDay(wd);
                var isExists = false;
                for (var i = 0; i < weekDaysSelected.length; i++) {
                    if (numberofday == weekDaysSelected[i]) {
                        isExists = true;
                    }
                }
                if (isExists) {
                    var strDate = d.toString('MM/dd/yyyy');
                    strDate = strDate.replace("/", "_");
                    strDate = strDate.replace("/", "_");
                    RecurringDates.push(strDate);

                    return d.toString('ddd MM/dd/yyyy');
                }
            }
        }
        else {
            if (document.getElementById('unit').value == 'm') {
                var wd = d.toString('ddd');
                var numberofday = getNumberOfDay(wd);
                if (document.getElementById('occurrence_of').value == numberofday) {
                    var strDate = d.toString('MM/dd/yyyy');
                    strDate = strDate.replace("/", "_");
                    strDate = strDate.replace("/", "_");
                    RecurringDates.push(strDate);

                    return d.toString('ddd MM/dd/yyyy');
                }
            }
            else {
                var strDate = d.toString('MM/dd/yyyy');
                strDate = strDate.replace("/", "_");
                strDate = strDate.replace("/", "_");
                RecurringDates.push(strDate);

                return d.toString('ddd MM/dd/yyyy');
            }
        }

    }).join("\n");
    checkstatus = "submit";
    // alert(checkstatus);
    //alert("here");
    //alert(document.getElementById('output').value);
    //if ($('output').value == 'Weekly recurrence was selected without any days specified.')
    //{
    //    alert("Please select at least one week day.");
    //}
    //else
    //{
    //    alert("ok");
    //    
    //}
   // alert(checkstatus);
    document.getElementById('btnClose').click();
    // $("#divProgress").hide();
    //   alert("end");
}

function getNumberOfDay(e) {
    switch (e) {
        case 'Sun':
            return 0;
            break;
        case 'Mon':
            return 1;
            break;
        case 'Tue':
            return 2;
            break;
        case 'Wed':
            return 3;
            break;
        case 'Thu':
            return 4;
            break;
        case 'Fri':
            return 5;
            break;
        case 'Sat':
            return 6;
            break;
        default:
            return '';
    }
}

function checkUnit() {


    var SelectedUnit = document.getElementById("unit").value;

    document.getElementById('week_span').style.display = 'none';
    document.getElementById('month_span').style.display = 'none';

    if (SelectedUnit == 'w') {
        document.getElementById('week_span').style.display = 'block';
    }
    if (SelectedUnit == 'm') {
        document.getElementById('month_span').style.display = 'block';

    }
}


function startSetUp()
{
    alert("start");
}
var today = new Date();
var dd = today.getDate();
var mm = today.getMonth() + 1; //January is 0!
var yyyy = today.getFullYear();
 

var mmnext = mm + 1;

 
var ddlstartDate = document.getElementById('startDate');
var optsstartDate = ddlstartDate.options.length;
for (var i = 0; i < optsstartDate; i++) {
    if (ddlstartDate.options[i].value == dd) {
        ddlstartDate.options[i].selected = true;
        break;
    }
}

var ddlendDate = document.getElementById('endDate');
var optsendDate = ddlendDate.options.length;
for (var i = 0; i < optsendDate; i++) {
    if (ddlendDate.options[i].value == dd) {
        ddlendDate.options[i].selected = true;
        break;
    }
}

var ddlstartMonth = document.getElementById('startMonth');
var optsstartMonth = ddlstartMonth.options.length;
for (var i = 0; i < optsstartMonth; i++) {
    if (ddlstartMonth.options[i].value == mm) {
        ddlstartMonth.options[i].selected = true;
        break;
    }
}

var ddlendMonth = document.getElementById('endMonth');
var optsendMonth = ddlendMonth.options.length;
for (var i = 0; i < optsendMonth; i++) {
    if (ddlendMonth.options[i].value == mmnext) {
        ddlendMonth.options[i].selected = true;
        break;
    }
}


var ddlstartYear = document.getElementById('startYear');
var optsstartYear = ddlstartYear.options.length;
for (var i = 0; i < optsstartYear; i++) {
    if (ddlstartYear.options[i].value == yyyy) {
        ddlstartYear.options[i].selected = true;
        break;
    }
}

var ddlendYear = document.getElementById('endYear');
var optsendYear = ddlendYear.options.length;
for (var i = 0; i < optsendYear; i++) {
    if (ddlendYear.options[i].value == yyyy) {
        ddlendYear.options[i].selected = true;
        break;
    }
}
