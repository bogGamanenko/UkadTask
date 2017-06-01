function updateChart() {
    google.charts.load("current", {
        packages: ['corechart'],
        callback: function () {
            $.ajax({
                type: "GET",
                url: "/Home/TestedUrls",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: drawChart
            });
        }
    });    
}

function drawChart(data, status) {   
    var chartDate = new google.visualization.DataTable();
    chartDate.addColumn('string', 'URL number');
    chartDate.addColumn('number', 'Request time');

    for (var i = 0; i < data.length; i++) {
        var array = [];
        array.push(i + 1 + '');
        array.push(data[i]);
        chartDate.addRow(array);
    }     

    var options = {      
        width: 600,
        height: 400,        
        bar: { groupWidth: "95%" },
        legend: { position: "none" },
        vAxis: {
            title: 'Request time (ms)'
        },
        hAxis: {
            title: 'URL number'
        }
    };

    var chartPanel = document.getElementById("chart_panel");
    chartPanel.style.display = "block";

    var chart = new google.visualization.ColumnChart(document.getElementById("column_chart"));
    chart.draw(chartDate, options);
}