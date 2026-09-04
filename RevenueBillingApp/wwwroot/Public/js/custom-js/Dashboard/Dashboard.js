$(document).ready(function () {
	new ApexCharts(document.querySelector("#payment-records-chart"), {
		chart: {
			height: 130,
			width: "100%",
			stacked: !1,
			toolbar: {
				show: !1
			}
		},
		stroke: {
			width: [1, 2, 3],
			curve: "smooth",
			lineCap: "round"
		},
		plotOptions: {
			bar: {
				endingShape: "rounded",
				columnWidth: "30%"
			}
		},
		colors: ["#0b3e21", "#0b3e21", "#0b3e21"],
		series: [
			{
				name: "Payment Completed",
				type: "line",
				data: [44, 55, 41, 67, 22, 43, 21, 41, 56, 27, 43, 41]
			}, {
				name: "Awaiting Payment",
				type: "bar",
				data: [44, 55, 41, 67, 22, 43, 21, 41, 56, 27, 43, 56]
			}],
		fill: {
			opacity: [.85, .25, 1, 1],
			gradient: {
				inverseColors: !1,
				shade: "light",
				type: "vertical",
				opacityFrom: .5,
				opacityTo: .1,
				stops: [0, 100, 100, 100]
			}
		},
		markers: {
			size: 0
		},
		xaxis: {
			categories: ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCt", "NOV", "DEC"],
			axisBorder: {
				show: !1
			},
			axisTicks: {
				show: !1
			},
			labels: {
				style: {
					fontSize: "10px",
					colors: "#A0ACBB"
				}
			}
		},
		yaxis: {
			tickAmount: 2,
			labels: {
				formatter: function (e) {
					return +e + "K"
				},
				offsetX: -5,
				offsetY: 0,
				style: {
					color: "#A0ACBB"
				}
			}
		},
		grid: {
			xaxis: {
				lines: {
					show: !1
				}
			},
			yaxis: {
				lines: {
					show: !1
				}
			}
		},
		dataLabels: {
			enabled: !1
		},
		tooltip: {
			y: {
				formatter: function (e) {
					return +e + "K"
				}
			},
			style: {
				fontSize: "12px",
				fontFamily: "Inter"
			}
		},
		legend: {
			show: !1,
			labels: {
				fontSize: "12px",
				colors: "#A0ACBB"
			},
			fontSize: "12px",
			fontFamily: "Inter"
		}
	}).render()
	new ApexCharts(document.querySelector("#logged-time-area-chart"), {
		chart: {
			height: 130,
			width: "100%",
			type: "area",
			stacked: !1,
			toolbar: {
				show: !1
			}
		},
		xaxis: {
			categories: ["2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022", "2023", "2024", "2025"],
			axisBorder: {
				show: !1
			},
			axisTicks: {
				show: !1
			},
			labels: {
				style: {
					fontSize: "11px",
					colors: "#64748b"
				}
			}
		},
		yaxis: {
			tickAmount: 3,
			labels: {
				formatter: function (e) {
					return +e + " %"
				},
				offsetX: -20,
				offsetY: 0,
				style: {
					fontSize: "11px",
					color: "#64748b"
				}
			}
		},
		stroke: {
			width: 2,
			curve: "smooth",
			lineCap: "round"
		},
		grid: {
			padding: {
				left: 0,
				right: 0
			},
			strokeDashArray: 3,
			borderColor: "#0b3e21",
			row: {
				colors: ["#0b3e21", "transparent"],
				opacity: .02
			}
		},
		legend: {
			show: !1
		},
		colors: ["#0b3e21"],
		dataLabels: {
			enabled: !1
		},
		fill: {
			type: "gradient",
			gradient: {
				shadeIntensity: 1,
				opacityFrom: .4,
				opacityTo: .8,
				stops: [0, 100]
			}
		},
		series: [{
			name: "Time Logged",
			data: [20, 45, 25, 60, 30, 65, 35, 75, 60, 80, 65],
			type: "area"
		}],
		tooltip: {
			y: {
				formatter: function (e) {
					return +e + " Mins"
				}
			},
			style: {
				fontSize: "11px",
				fontFamily: "Inter"
			}
		}
	}).render()
})
