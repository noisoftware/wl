$(function () {
    $("#txtWaterSupplyOperator").autocomplete({
        source: function (request, response) {
            var OperatorType = "WATER";
            $.ajax({
                url: '/MerchantRechargeService/AutoWaterBillService',
                data: "{ 'prefix': '" + request.term + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data, function (item) {
                        return item;
                    }))
                },
                error: function (response) {
                    console.log(response);
                    alert(response.responseText);

                },
                failure: function (response) {
                    console.log(response);
                    alert(response.responseText);
                }
            });
        },
        select: function (e, i) {
            $("#hfWaterSupplyperator").val(i.item.val);
        },
        minLength: 1
    });
});

//$(document).ready(function () {
//    $("#PrepaidRecharge1").change(function () {
//        debugger;
//        $('#txtOperator').val('');
//    });
//    $("#PostpaidRecharge2").change(function () {

//        $('#txtOperator').val('');
//    });
//});

