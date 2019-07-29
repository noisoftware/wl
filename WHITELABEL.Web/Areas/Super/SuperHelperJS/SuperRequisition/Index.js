﻿
function DeactivateTransactionlist(id) {
    bootbox.confirm({
        title: "Message",
        message: "Do you want to deactivate Transaction information",
        buttons: {
            confirm: {
                label: 'Confirm',
                className: 'btn-success btn-sm'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-danger btn-sm'
            }
        },
        callback: function (result) {
            if (result) {
                var token = $(':input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: "/SuperRequisition/DeactivateTransactionDetails?area=Super",
                    data: {
                        __RequestVerificationToken: token,
                        transid: id
                    },
                    cache: false,
                    type: "POST",
                    dataType: "json",
                    beforeSend: function () {
                    },
                    success: function (data) {

                        if (data.Result === "true") {
                            $('.mvc-grid').mvcgrid('reload');
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                size: "small",
                                message: "Status changed successfully",
                                backdrop: true
                            });

                        }
                        else {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "there is some thing error",
                                backdrop: true
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log(status);
                    }
                });
            }
        }
    });
}

function updateStatus(transid) {
    bootbox.confirm({
        title: "Message",
        message: "Do you want to Approve Transaction",
        buttons: {
            confirm: {
                label: 'Confirm',
                className: 'btn-success btn-sm'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-danger btn-sm'
            }
        },
        callback: function (result) {
            if (result) {
                //var trandate = $("#TransactionDate").val();
                //var TransationStatus = $("#TransationStatus").val();
                var TransationStatus = "1";
                //var slnval = $("#sln").val();
                var slnval = transid;
                var token = $(':input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: "/SuperRequisition/ChangeTransactionStatus?area=Super",
                    data: {
                        __RequestVerificationToken: token,
                        slnval: slnval
                    },
                    cache: false,
                    type: "POST",
                    dataType: "json",
                    beforeSend: function () {
                    },
                    success: function (data) {

                        if (data.Result === "true") {
                            $('.mvc-grid').mvcgrid('reload');
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                size: "small",
                                message: "Transaction approved",
                                backdrop: true
                            });
                            $('#transactmodal').modal('hide');
                        }
                        else if (data.Result === "Pending") {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "No succificient balance to transfer",
                                backdrop: true
                            });
                        }
                        else {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "there is some thing error",
                                backdrop: true
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log(status);
                    }
                });
            }
        }
    });


    //var trandate = $("#TransactionDate").val();
    ////var TransationStatus = $("#TransationStatus").val();
    //var TransationStatus = "1";
    ////var slnval = $("#sln").val();
    //var slnval = transid;
    //var token = $(':input[name="__RequestVerificationToken"]').val();
    //$.ajax({
    //    url: "/SuperRequisition/ChangeTransactionStatus?area=Super",
    //    data: {
    //        __RequestVerificationToken: token,
    //        trandate: trandate, TransationStatus: TransationStatus, slnval: slnval
    //    },
    //    cache: false,
    //    type: "POST",
    //    dataType: "json",
    //    beforeSend: function () {
    //    },
    //    success: function (data) {

    //        if (data.Result === "true") {
    //            $('.mvc-grid').mvcgrid('reload');
    //            $(".overlaydiv").fadeOut("slow");
    //            bootbox.alert({
    //                size: "small",
    //                message: "Transaction approved",
    //                backdrop: true
    //            });
    //            $('#transactmodal').modal('hide');
    //        }
    //        else if (data.Result === "Pending") {
    //            $(".overlaydiv").fadeOut("slow");
    //            bootbox.alert({
    //                message: "No succificient balance to transfer",
    //                backdrop: true
    //            });
    //        }
    //        else {
    //            $(".overlaydiv").fadeOut("slow");
    //            bootbox.alert({
    //                message: "there is some thing error",
    //                backdrop: true
    //            });
    //        }
    //    },
    //    error: function (xhr, status, error) {
    //        console.log(status);
    //    }
    //});
}


function TransactionDecline(transid) {    
    bootbox.confirm({
        title: "Message",
        message: "Do you want to deactivate Transaction information",
        buttons: {
            confirm: {
                label: 'Confirm',
                className: 'btn-success btn-sm'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-danger btn-sm'
            }
        },
        callback: function (result) {
            if (result) {
                //var trandate = $("#TransactionDate").val();
                //var TransationStatus = $("#TransationStatus").val();
                var TransationStatus = "0";
                //var slnval = $("#sln").val();
                var slnval = transid;
                var token = $(':input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: "/SuperRequisition/TransactionDecline?area=Super",
                    data: {
                        __RequestVerificationToken: token,
                        slnval: slnval
                    },
                    cache: false,
                    type: "POST",
                    dataType: "json",
                    beforeSend: function () {
                    },
                    success: function (data) {

                        if (data.Result === "true") {
                            $('.mvc-grid').mvcgrid('reload');
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                size: "small",
                                message: "Transaction declined",
                                backdrop: true
                            });
                            $('#transactmodal').modal('hide');
                        }
                        else {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "there is some thing error",
                                backdrop: true
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log(status);
                    }
                });
            }
        }
    });
    ////var trandate = $("#TransactionDate").val();
    ////var TransationStatus = $("#TransationStatus").val();
    //var TransationStatus = "0";
    ////var slnval = $("#sln").val();
    //var slnval = transid;
    //var token = $(':input[name="__RequestVerificationToken"]').val();
    //$.ajax({
    //    url: "/SuperRequisition/TransactionDecline?area=Super",
    //    data: {
    //        __RequestVerificationToken: token,
    //        slnval: slnval
    //    },
    //    cache: false,
    //    type: "POST",
    //    dataType: "json",
    //    beforeSend: function () {
    //    },
    //    success: function (data) {

    //        if (data.Result === "true") {
    //            $('.mvc-grid').mvcgrid('reload');
    //            $(".overlaydiv").fadeOut("slow");
    //            bootbox.alert({
    //                size: "small",
    //                message: "Transaction declined",
    //                backdrop: true
    //            });
    //            $('#transactmodal').modal('hide');
    //        }
    //        else {
    //            $(".overlaydiv").fadeOut("slow");
    //            bootbox.alert({
    //                message: "there is some thing error",
    //                backdrop: true
    //            });
    //        }
    //    },
    //    error: function (xhr, status, error) {
    //        console.log(status);
    //    }
    //});
}

function getvalue(transid) {
    var idval = transid;
    $.ajax({
        url: "/SuperRequisition/getTransdata?area=Super",
   // url: "@Url.Action("getTransdata", "SuperRequisition", new {area= "Super" })",
        data: {
        TransId: transid
    },
    cache: false,
        type: "POST",
            dataType: "json",
                beforeSend: function () {
                },
    success: function (data) {
        if (data.Result === "true") {
            //debugger;
            var traninfo = data;
            //var dateval = new Date(traninfo.data.REQUEST_DATE)
            $('#username').val(traninfo.data.FromUser);
            $('#TransactionDate').val(formatDate(traninfo.data.REQUEST_DATE));
            $('#sln').val(traninfo.data.SLN);
            $("#BankDetails").val(traninfo.data.BANK_ACCOUNT);
            $("#Amount").val(traninfo.data.AMOUNT);
            //document.getElementById("username").innerHTML = traninfo.data.AMOUNT;
        }
        else {
            $(".overlaydiv").fadeOut("slow");

        }
    },
    error: function (xhr, status, error) {
        console.log(status);
    }
});
    }

function formatDate(inputDate) {
    var value = new Date(parseInt(inputDate.replace(/(^.*\()|([+-].*$)/g, '')));
    var formattedDate = value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear();
    return formattedDate;
}

$(function () {

    $('#txtReferenceNumber').on('input blur change keyup', function () {
        //debugger;
        if ($(this).val().length != 0) {
            var token = $(':input[name="__RequestVerificationToken"]').val();
            $.ajax({
                //url: "@Url.Action("CheckEmailAvailability")",
                url: '/SuperRequisition/CheckReferenceNo?area=Super',
                data: {
                    referenceno: $(this).val(),
                    __RequestVerificationToken: token
                },
                cache: false,
                type: "POST",
                success: function (data) {
                    // DONE
                    //debugger;
                    if (data.result == "available") {
                        $('#txtReferenceNumber').css('border', '3px #090 solid');
                        //$('#btnsubmit').attr('disabled', false);
                    }
                    else {
                       // debugger;
                        $('#txtReferenceNumber').css('border', '3px #C33 solid');
                        //$('#txtMemberDomain').val(data.Mem_Name);                    
                        //$('#txtMem_ID').val(data.mem_Id);
                        //document.getElementById('txtMem_ID').value = data.mem_Id;
                        //$('#txtAmount').val(data.amt);
                        //$('#txtRequestDate').val(data.Req_Date);
                        //$('#BankID').val(data.Bankid);
                        //$('#Paymentmethod').val(data.paymethod);
                        //$('#txtTransactiondetails').val(data.Transdetails);
                        //$('#txtBankCharges').val(data.BankCharges);

                        //$('#btnsubmit').attr('disabled', true);
                        //alert("This email id is already registered");
                    }
                },
                error: function (xhr, status, error) {
                    console.log(xhr.responseText);
                    alert("message : \n" + "An error occurred" + "\n status : \n" + status + " \n error : \n" + error);
                }
            });
        }
        else {
            // $('#btnsubmit').attr('disabled', true);
        }
    });

});

$(function () {

    $('#txtRequestsuperReferenceNumber').on('input blur change keyup', function () {
        //debugger;
        if ($(this).val().length != 0) {
            var token = $(':input[name="__RequestVerificationToken"]').val();
            $.ajax({
                //url: "@Url.Action("CheckEmailAvailability")",
                url: '/SuperRequestRequisition/CheckReferenceNo?area=Super',
                data: {
                    referenceno: $(this).val(),
                    __RequestVerificationToken: token
                },
                cache: false,
                type: "POST",
                success: function (data) {
                    // DONE
                    //debugger;
                    if (data.result == "available") {
                        $('#txtRequestsuperReferenceNumber').css('border', '3px #090 solid');
                        //$('#btnsubmit').attr('disabled', false);
                    }
                    else {
                       // debugger;
                        $('#txtRequestsuperReferenceNumber').css('border', '3px #C33 solid');
                        //$('#txtMemberDomain').val(data.Mem_Name);                    
                        //$('#txtMem_ID').val(data.mem_Id);
                        //document.getElementById('txtMem_ID').value = data.mem_Id;
                        //$('#txtAmount').val(data.amt);
                        //$('#txtRequestDate').val(data.Req_Date);
                        //$('#BankID').val(data.Bankid);
                        //$('#Paymentmethod').val(data.paymethod);
                        //$('#txtTransactiondetails').val(data.Transdetails);
                        //$('#txtBankCharges').val(data.BankCharges);

                        //$('#btnsubmit').attr('disabled', true);
                        //alert("This email id is already registered");
                    }
                },
                error: function (xhr, status, error) {
                    console.log(xhr.responseText);
                    alert("message : \n" + "An error occurred" + "\n status : \n" + status + " \n error : \n" + error);
                }
            });
        }
        else {
            // $('#btnsubmit').attr('disabled', true);
        }
    });

});


function ActivateRequisition(transid) {
    bootbox.confirm({
        title: "Message",
        message: "Do you want to Activate Requisition",
        buttons: {
            confirm: {
                label: 'Confirm',
                className: 'btn-success btn-sm'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-danger btn-sm'
            }
        },
        callback: function (result) {
            if (result) {
                //var trandate = $("#TransactionDate").val();
                //var TransationStatus = $("#TransationStatus").val();
                var TransationStatus = "1";
                //var slnval = $("#sln").val();
                var slnval = transid;
                var token = $(':input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: "/SuperRequisition/ActivateRequisition?area=Super",
                    data: {
                        __RequestVerificationToken: token,
                        slnval: slnval
                    },
                    cache: false,
                    type: "POST",
                    dataType: "json",
                    beforeSend: function () {
                    },
                    success: function (data) {

                        if (data.Result === "true") {
                            $('.mvc-grid').mvcgrid('reload');
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                size: "small",
                                message: "Requisition Activated.",
                                backdrop: true
                            });
                            $('#transactmodal').modal('hide');
                        }
                        else if (data.Result === "Pending") {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "No succificient balance to transfer",
                                backdrop: true
                            });
                        }
                        else {
                            $(".overlaydiv").fadeOut("slow");
                            bootbox.alert({
                                message: "there is some thing error",
                                backdrop: true
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log(status);
                    }
                });
            }
        }
    });
    
}