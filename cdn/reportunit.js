var menuWidth = 260;

function showDynamicModal(heading, content) {
    var m = $('#dynamicModal');
    m.find('h4').text(heading);
    m.find('pre').text(content);
    m.openModal({ in_duration: 200 });
}

$('.details-container').click(function(evt) {
    var t = $(evt.target);

    if (t.is('.showStatusMessage') || t.is('i')) {
        if (t.is('i')) {
            t = t.parent();
        }

        showDynamicModal(t.closest('tr').find('.name').text() + ' StatusMessage', t.next().text());
    }
    
    if (t.is('.showDescription')) {
        showDynamicModal(t.text() + ' Description', t.next().text());
    }
});

/* toggle dashboard on 'Enable Dashboard' click */
$('#enableDashboard').click(function() {
    $(this).toggleClass('enabled').children('i').toggleClass('active');
    $('.dashboard').toggleClass('hide');
});

/* show suite data on click */
$('.suite').click(function() {
    var t = $(this);
    
    $('.suite').removeClass('active');
    $('.suite-name-displayed, .details-container').html('');
    
    t.toggleClass('active');

    var html = t.find('.suite-content').html();
    var suiteName = t.find('.suite-name').text();

    $('.suite-name-displayed').text(suiteName).attr('title', suiteName);
    $('.details-container').append(html);
    initCategoryFilter();
});

$('#nav-mobile .report-item > a').filter(function(){
    return this.href.match(/[^\/]+$/)[0] == document.location.pathname.match(/[^\/]+$/)[0];
}).parent().addClass('active');


function initSuiteFilter() {
    $('.filter-suites').click(function () {
        var t = $(this);
        var $dd = $('#suite-toggle');
        filterBySuites(t, t.text(), $dd.attr('data-filter'), $dd.attr('data-filter-display'));
    });
}

/* filters -> by suite status */
function filterBySuites(t, status, filter, filterDisplay) {    
    
    if (!t.hasClass('clear')) {
        resetFilters();
        
        var lowerStatus = status.toLowerCase();

        $('#suites .suite').addClass('hide');
        $('#suites .suite.' + lowerStatus).removeClass('hide');
        
        filterTests(lowerStatus, status, filter, filterDisplay);
        selectVisSuite()
    }
}

function initTestFilter() {
    $('.filter-tests').click(function () {
        var t = $(this);
        var $dd = $('#tests-toggle');
        filterByTests(t, t.text(), $dd.attr('data-filter'), $dd.attr('data-filter-display'));
    });
}

/* filters -> by test status */
function filterByTests(t, status, filter, filterDisplay) {
   
    if (!t.hasClass('clear')) {
        resetFilters();

        var opt = status;
        var lowerOpt = opt.toLowerCase();

        $('.suite table tr.test-status:not(.' + lowerOpt + '), .details-container tr.test-status:not(.' + lowerOpt).addClass('hide');
        $('.suite table tr.test-status.' + lowerOpt + ', .details-container tr.test-status.' + lowerOpt).removeClass('hide');

        filterTests(lowerOpt, opt, filter, filterDisplay);
        selectVisSuite()
    }
}

function initCategoryFilter() {
    $('.filter-categories').click(function () {
        var t = $(this);
        var $dd = $('#category-toggle');
        filterByCategories(t, t.text(), $dd.attr('data-filter'), $dd.attr('data-filter-display'));
    });
}

/* filters -> by category */
function filterByCategories(t, status, filter, filterDisplay) {
            
    if (!t.hasClass('clear')) {
        resetFilters();
        filterTests(status, status, filter, filterDisplay);
        selectVisSuite()
    }
}

/* The function that takes the table rows and hides and shows appropriately */
function filterTests(cat, displayCat, filterType, display) {

    $('td.test-features').each(function () {        
        if (!($(this).hasClass(cat))) {
            $(this).closest('tr').addClass('hide');            
        }
    });    

    displayFilterApplied(displayCat, filterType, display);
    hideEmptySuites();
    initClear();
}

function initClear() {
    $('.clear').click(function () {
        resetFilters(); selectVisSuite()
    });
}

function displayFilterApplied(cat, filterType, display) {
    var $fa = $('#filters-applied');
    $fa.removeClass('hide');
    $fa.find('.' + filterType + '-chip').remove();
    $fa.append('<div class="chip ' + filterType + '-chip">' + display + cat + '<i class="material-icons clear">close</i><div>');
}

function hideEmptySuites() {

    $('.suite').each(function () {
        var t = $(this);        
        
        if (t.find('tr.test-status').length == t.find('tr.test-status.hide').length) {
            t.addClass('hide');
        }
    });
}

function resetFilters() {
    $('.suite, tr.test-status').removeClass('hide');
    $('.suite-toggle li:first-child, .tests-toggle li:first-child, .feature-toggle li:first-child').click();
    $('#filters-applied').addClass('hide').find('.chip').remove();
}

function selectVisSuite() {
    $('.suite:visible').get(0).click();
}

function clickListItem(listClass, index) {
    $('#' + listClass).find('li').get(index).click();
}

$(document).ready(function() {
	/* init */
    $('select').material_select();
    $('.modal-trigger').leanModal({
        dismissible: true, // Modal can be dismissed by clicking outside of the modal
        opacity: .5, // Opacity of modal background
        in_duration: 300, // Transition in duration
        out_duration: 200 // Transition out duration
    }
  );
    $('.tooltipped').tooltip({ delay: 10 });

	var passedPercentage = Math.round(((passed / total) * 100)) + '%';
	$('.pass-percentage').text(passedPercentage);
	$('.dashboard .determinate').attr('style', 'width:' + passedPercentage);

	suitesChart(); testsChart();
	$('ul.doughnut-legend').addClass('right');

	initSuiteFilter();
	initTestFilter();
	initCategoryFilter();
	
	resetFilters();
	$('.suite:first-child').click();
    // Initialize collapse button
	$(".button-collapse").sideNav();
	$("#button-shrink").click(function (e) {
	    e.stopPropagation();
	    $("#nav-mobile").toggleClass("collapse");
	});
});

var options = {
	segmentShowStroke : false, 
	percentageInnerCutout : 55, 
	animationSteps : 1,
	legendTemplate : '<ul class=\'<%=name.toLowerCase()%>-legend\'><% for (var i=0; i<segments.length; i++){%><li><span style=\'background-color:<%=segments[i].fillColor%>\'></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>'
};

function toggleMenu() {
    var isAnimationRunning = false;
}

/* report -> suites chart */
function suitesChart() {

    var suiteAnalysis = $('#suite-analysis');
	if (!$('#suite-analysis').length) {
		return false;
	}

	var passed = $('.suite-result.passed').length;
	var failed = $('.suite-result.failed').length;
	var others = $('.suite-result.error, .suite-result.inconclusive, .suite-result.skipped').length;

	$('.suite-pass-count').text(passed);
	$('.suite-fail-count').text(failed);
	$('.suite-others-count').text(others);
	
	var data = [
		{ value: passed, color: '#00af00', highlight: '#32bf32', label: 'Passed' },
		{ value: failed, color:'#F7464A', highlight: '#FF5A5E', label: 'Failed' },
		{ value: $('.suite-result.error').length, color:'#ff6347', highlight: '#ff826b', label: 'Error' },
		{ value: $('.suite-result.inconclusive').length, color: '#FDB45C', highlight: '#FFC870', label: 'Inconclusive' },
		{ value: $('.suite-result.skipped').length, color: '#1e90ff', highlight: '#4aa6ff', label: 'Skipped' }
	];
	
	var ctx = suiteAnalysis.get(0).getContext('2d');
	var suiteChart = new Chart(ctx).Doughnut(data, options);
	drawLegend(suiteChart, 'suite-analysis');

	var suiteToggle = $('#suite-toggle');

	suiteAnalysis.click(
        function(evt) {
            var activePoints = suiteChart.getSegmentsAtEvent(evt);
            var firstPoint = activePoints[0];            
            filterBySuites($(this), firstPoint.label, suiteToggle.attr('data-filter'), suiteToggle.attr('data-filter-display'));
        }
    )
}

/* test case counts */
var $suites = $('#suites');
var total = $suites.attr('data-total');
var passed = $suites.attr('data-passed');
var failed = $suites.attr('data-failed');
var inconclusive = $suites.attr('data-inconclusive');
var errors = $suites.attr('data-errors');
var skipped = $suites.attr('data-skipped');

/* report -> tests chart */
function testsChart() {
    var testAnalysis = $('#test-analysis');
    if (!testAnalysis.length) {
		return false;
	}

    var data = {};
    
    if ($('body.summary').length > 0) {
        total = parseInt($('#total-tests').text(), 10);
        passed = parseInt($('#total-passed').text(), 10);
        failed = parseInt($('#total-failed').text(), 10);
        others = parseInt($('#total-others').text(), 10);
        
        data = [
            { value: passed, color: '#00af00', highlight: '#32bf32', label: 'Passed' },
            { value: failed, color:'#F7464A', highlight: '#FF5A5E', label: 'Failed' },
            { value: others, color: '#1e90ff', highlight: '#4aa6ff', label: 'Others' }
        ];
        
        $('.test-others-count').text(others);
    }
    else {
        data = [
            { value: passed, color: '#00af00', highlight: '#32bf32', label: 'Passed' },
            { value: failed, color:'#F7464A', highlight: '#FF5A5E', label: 'Failed' },
            { value: errors, color:'#ff6347', highlight: '#ff826b', label: 'Error' },
            { value: inconclusive, color: '#FDB45C', highlight: '#FFC870', label: 'Inconclusive' },
            { value: skipped, color: '#1e90ff', highlight: '#4aa6ff', label: 'Skipped' }
        ];
        
        $('.test-others-count').text(parseInt(errors, 10) + parseInt(inconclusive, 10) + parseInt(skipped, 10));
    }
	
	$('.test-pass-count').text(passed);
	$('.test-fail-count').text(failed);
	
	var ctx = testAnalysis.get(0).getContext('2d');
	testChart = new Chart(ctx).Doughnut(data, options);
	drawLegend(testChart, 'test-analysis');

	var testsToggle = $('#tests-toggle');

	testAnalysis.click(
        function (evt) {
            var activePoints = testChart.getSegmentsAtEvent(evt);
            var firstPoint = activePoints[0];
            filterByTests($(this), firstPoint.label, testsToggle.attr('data-filter'), testsToggle.attr('data-filter-display'));
        }
    )
}

/* draw legend for test and step charts [DASHBOARD] */
function drawLegend(chart, id) {
	var helpers = Chart.helpers;
	var legendHolder = document.getElementById(id);
	
	legendHolder.innerHTML = chart.generateLegend();
	
	helpers.each(legendHolder.firstChild.childNodes, function(legendNode, index) {
		helpers.addEvent(legendNode, 'mouseover', function() {
			var activeSegment = chart.segments[index];
			activeSegment.save();
			activeSegment.fillColor = activeSegment.highlightColor;
			chart.showTooltip([activeSegment]);
			activeSegment.restore();
		});
	});
	
	Chart.helpers.addEvent(legendHolder.firstChild, 'mouseout', function() {
		chart.draw();
	});
	
	$('#' + id).after(legendHolder.firstChild);
}
