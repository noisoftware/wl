$(function () {
    //debugger;
    //var PrepaidRecharge = $('#PrepaidRecharge1').val(); 
    //var PostpaidRecharge = $('#PostpaidRecharge2').val();
    //var OperatorType = "";
    //if (document.getElementById("PrepaidRecharge1").checked)
    //{
    //    OperatorType = "Prepaid";
    //}
    //if (document.getElementById("PostpaidRecharge2").checked) {
    //    OperatorType = "Postpaid";
    //}
    $("#txtOperator").autocomplete({
        source: function (request, response) {
            var OperatorType = "";
            if (document.getElementById("PrepaidRecharge1").checked) {
                OperatorType = "Prepaid";
            }
            if (document.getElementById("PostpaidRecharge2").checked) {
                OperatorType = "POSTPAID";
            }
            $.ajax({
                url: '/MerchantRechargeService/AutoComplete/',
                data: "{ 'prefix': '" + request.term + "','OperatorType':'" + OperatorType + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data, function (item) {
                        return item;
                    }))
                },
                error: function (response) {
                    alert(response.responseText);
                },
                failure: function (response) {
                    alert(response.responseText);
                }
            });
        },
        select: function (e, i) {
            $("#hfOperator").val(i.item.val);
        },
        minLength: 1
    });
});

$(document).ready(function () {
    $("#PrepaidRecharge1").change(function () {
        //debugger;
        $('#txtOperator').val('');
    });
    $("#PostpaidRecharge2").change(function () {

        $('#txtOperator').val('');
    });
});

