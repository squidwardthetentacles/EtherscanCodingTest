// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var getToken = null;
var setDataAndOpenViewTokenModal = null;
var setDataAndOpenEditTokenModal = null;
var save = null;
var reset = null;
var table = null;

(function ($) {
    $(document).ready(function () {
        function init() {
            initDataTable();
            getAllTotalSupplyPercentage(initHighChart);
        };

        var getAllToken = function (skipCount, maxResultCount, callback) {
            $.ajax({
                "async": true,
                "crossDomain": true,
                "url": "https://localhost:7266/Token/GetAll",
                "method": "GET",
                "headers": {
                    "content-type": "application/json"
                },
                "data": {
                    "skipCount": skipCount,
                    "maxResultCount": maxResultCount
                }
            }).done(function (response) {
                console.log(response);

                if (response.message) {
                    Swal.fire(
                        'Failed',
                        'Something went wrong',
                        'error'
                    );

                    return;
                }

                if (callback) {
                    callback({
                        recordsTotal: response.totalCount,
                        recordsFiltered: response.totalCount,
                        data: response.tokens,
                    });
                }
            }).fail(function () {
                Swal.fire(
                    'Failed',
                    'Something went wrong',
                    'error'
                );
            });
        };

        var initDataTable = function () {
            
            table = $('#token-datatable').DataTable({
                destroy: true,
                dom: 'Bfrtip',
                pageLength: 10,
                lengthChange: false,
                ordering: false,
                info: true,
                bFilter: false,
                processing: true,
                serverSide: true,
                ajax: function (data, callback) {
                    let skipCount = data.start;
                    let maxResultCount = data.length;

                    getAllToken(skipCount, maxResultCount, callback);
                },
                columns: [
                    { data: "id", visible: false },
                    { data: "rank" },
                    {
                        data: "symbol",
                        fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                            $(nTd).html(`<a href='javascript:void(0)' onclick='getToken(${oData.id}, setDataAndOpenViewTokenModal)'>${sData}</a>`);
                        }
                    },
                    { data: "name" },
                    { data: "contractAddress" },
                    { data: "totalHolders" },
                    { data: "totalSupply" },
                    { data: "totalSupplyPercentage" },
                    {
                        data: "id",
                        fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                            $(nTd).html(`<a href='javascript:void(0)' onclick='getToken(${oData.id}, setDataAndOpenEditTokenModal)'>Edit</a>`);
                        }
                    }
                ],
                buttons: [{ extend: 'csv', text: 'Export CSV' }]
            });
        };

        getToken = function (id, callback) {
            $.ajax({
                "async": true,
                "crossDomain": true,
                "url": "https://localhost:7266/Token/Get",
                "method": "GET",
                "headers": {
                    "content-type": "application/json"
                },
                "data": {
                    "id": id
                }
            }).done(function (response) {
                console.log(response);

                if (response.message) {
                    Swal.fire(
                        'Failed',
                        'Something went wrong',
                        'error'
                    );

                    return;
                }

                if (callback) {
                    let token = response.token;
                    callback(token);
                }
            }).fail(function () {
                Swal.fire(
                    'Failed',
                    'Something went wrong',
                    'error'
                );
            });
        };

        setDataAndOpenViewTokenModal = function (token) {
            $("#viewContractAddress").text(token.contractAddress);
            $("#viewPrice").text(`$ ${token.price}`);
            $("#viewTotalSupply").text(`${token.totalSupply} ${token.symbol}`);
            $("#viewTotalHolders").text(token.totalHolders);
            $("#viewName").text(token.name);

            openViewTokenModal();
        };

        var openViewTokenModal = function () {
            let options = {
                backdrop: true,
                keyboard: true
            };
            let modal = new bootstrap.Modal($("#viewTokenModal"), options);
            modal.show();
        }

        setDataAndOpenEditTokenModal = function (token) {
            $("#editTokenId").val(token.id);
            $("#editTokenName").val(token.name);
            $("#editTokenSymbol").val(token.symbol);
            $("#editTokenContractAddress").val(token.contractAddress);
            $("#editTokenTotalSupply").val(token.totalSupply);
            $("#editTokenTotalHolders").val(token.totalHolders);

            openEditTokenModal();
        };

        var openEditTokenModal = function () {
            $("#editTokenModal").modal('show');
        }

        var closeEditTokenModal = function () {
            $("#editTokenModal").modal('hide');
        }

        save = function (hasId) {
            let token = {
                Id: 0,
                Symbol: "",
                Name: "",
                TotalSupply: 0,
                ContractAddress: "",
                TotalHolders: 0
            };

            if (hasId) {
                token.Id = $("#editTokenId").val();
                token.Name = $("#editTokenName").val();
                token.Symbol = $("#editTokenSymbol").val();
                token.ContractAddress = $("#editTokenContractAddress").val();
                token.TotalSupply = $("#editTokenTotalSupply").val();
                token.TotalHolders = $("#editTokenTotalHolders").val();
            }
            else {
                token.Name = $("#createTokenName").val();
                token.Symbol = $("#createTokenSymbol").val();
                token.ContractAddress = $("#createTokenContractAddress").val();
                token.TotalSupply = $("#createTokenTotalSupply").val();
                token.TotalHolders = $("#createTokenTotalHolders").val();
            }
            
            createOrUpdateToken(token);
        }

        reset = function () {
            $("#createTokenName").val("");
            $("#createTokenSymbol").val("");
            $("#createTokenContractAddress").val("");
            $("#createTokenTotalSupply").val("");
            $("#createTokenTotalHolders").val("");
        };

        var createOrUpdateToken = function (token) {
            $.ajax({
                "async": true,
                "crossDomain": true,
                "url": "https://localhost:7266/Token/CreateOrUpdate",
                "method": "POST",
                "headers": {
                    "content-type": "application/json"
                },
                dataType: "json",
                "data": JSON.stringify(token)
            }).done(function (response) {
                console.log(response);

                if (response.success) {
                    reset();
                    closeEditTokenModal();
                    init();

                    Swal.fire(
                        'Saved',
                        '',
                        'success'
                    );
                }
                else {
                    Swal.fire(
                        'Failed',
                        'Something went wrong',
                        'error'
                    );
                }

            }).fail(function () {
                Swal.fire(
                    'Failed',
                    'Something went wrong',
                    'error'
                );
            });
        }

        var initHighChart = function (data) {
            Highcharts.chart('chart', {
                credits: {
                    enabled: false
                },
                chart: {
                    plotBackgroundColor: null,
                    plotBorderWidth: null,
                    plotShadow: false,
                    type: 'pie'
                },
                title: {
                    text: 'Token Statistics by Total Supply'
                },
                tooltip: {
                    pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                },
                accessibility: {
                    point: {
                        valueSuffix: '%'
                    }
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        cursor: 'pointer',
                        dataLabels: {
                            enabled: true,
                            format: '<b>{point.name}</b>: {point.percentage:.1f} %'
                        },
                        innerSize: '40%'
                    }
                },
                series: [{
                    name: 'Total Supply',
                    colorByPoint: true,
                    data: data
                }]
            });
        };

        var getAllTotalSupplyPercentage = function (callback) {
            $.ajax({
                "async": true,
                "crossDomain": true,
                "url": "https://localhost:7266/Token/GetAllTotalSupplyPercentage",
                "method": "GET",
                "headers": {
                    "content-type": "application/json"
                }
            }).done(function (response) {
                console.log(response);

                if (response.message) {
                    Swal.fire(
                        'Failed',
                        'Something went wrong',
                        'error'
                    );

                    return;
                }

                if (callback) {
                    if (!response.tokenTotalSupplyPercentages) {
                        return;
                    }

                    let data = response.tokenTotalSupplyPercentages.map(function (o) {
                        return {
                            name: o.name,
                            y: o.totalSupplyPercentage
                        };
                    });

                    callback(data);
                }
            }).fail(function () {
                Swal.fire(
                    'Failed',
                    'Something went wrong',
                    'error'
                );
            });
        };

        init();
    });

})(jQuery);