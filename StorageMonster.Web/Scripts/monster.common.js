﻿var MonsterApp = {};

MonsterApp.CssSelectors = {
    ModalLogonId: ' #modalLogon ',
    ModalLogonBodyClass: ' .modal-body ',
    ErrorInfoBlock: ' div[data-errorinfo] '
};

function lockFormLogon() {
    $('<div>')
        .addClass('loader')
        .appendTo(MonsterApp.CssSelectors.ModalLogonId);
}
function unlockFormLogon() {
    $('div.loader').remove();
    $(MonsterApp.CssSelectors.ModalLogonId).modal('hide');
}

function showLogOnForm(html) {
    $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ModalLogonBodyClass).html(html);
    $.validator.unobtrusive.parse($(MonsterApp.CssSelectors.ModalLogonId));
    $(MonsterApp.CssSelectors.ModalLogonId).modal('show');
    $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ModalLogonBodyClass + 'form').on('submit', function (e) {
        e.preventDefault();
        var form = $(MonsterApp.CssSelectors.ModalLogonId + 'form');
        if (!form.valid())
            return;
        var url = form.attr('action');
        var data = form.serialize();
        MonsterApp.Ajax({
            url: url,
            data: data,
            dataType: 'json',
            type: 'post',
            success: function(responce) {
                if (typeof responce.Authorized != 'undefined' && responce.Authorized) {
                    unlockFormLogon();
                }

            },
            error: function (responce) {
                var message = MonsterApp.Messages.ServerError;
                if (typeof responce.Error != 'undefined') 
                    message = responce.Error;
                $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ErrorInfoBlock).text(message).show();
                $('div.loader').remove();
            },
            beforeSend: function() {
                lockFormLogon();
            }
        });
    });
}

MonsterApp.Ajax = function (options) {
    if (!options || typeof options != 'object')
        throw MonsterApp.Messages.AjaxOptionsFail;
    $.ajax({
        url: options.url,
        cache: options.cache,
        data: options.data,
        dataType: options.dataType,
        type: options.type,
        beforeSend: function(params) {
            if ($.isFunction(options.beforeSend))
                options.beforeSend(params);
        },
        error: function(error) {
            if ($.isFunction(options.error))
                options.error(error);
        },
        success: function (success) {
            if (typeof success.Error != 'undefined') {
                if ($.isFunction(options.error)) 
                    options.error(success);
                return;
            }
            if (typeof success.Authorized != 'undefined' && success.Authorized === false) {
                showLogOnForm(success.LogOnPage);
                $('div.loader').remove();
                return;
            }
            if ($.isFunction(options.success))
                options.success(success);
        },
        complete: function(complete) {
            if ($.isFunction(options.complete))
                options.complete(complete);
        }
    });
};