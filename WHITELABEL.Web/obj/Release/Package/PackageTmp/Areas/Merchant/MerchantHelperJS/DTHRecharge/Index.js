﻿$(function () {
    $("#txtDTHOperator").autocomplete({
        source: function (request, response) {         
            $.ajax({
                url: '/MerchantRechargeService/AutoDTHRechargeComplete/',
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
                    alert(response.responseText);
                },
                failure: function (response) {
                    alert(response.responseText);
                }
            });
        },
        select: function (e, i) {
            $("#hfDTHOperator").val(i.item.val);
        },
        minLength: 1
    });
});