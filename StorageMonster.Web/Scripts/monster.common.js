var MonsterApp;
(function (MonsterApp) {
    MonsterApp.TemplateNames = {
        MasterViewTmpl: "MasterViewTmpl",
        ProfileTmpl: "ProfileTmpl"
    };
    MonsterApp.CssSelectors = {
        ModalLogonId: ' #modalLogon ',
        ModalLogonBodyClass: ' .modal-body ',
        ModalLogonHeaderId: ' #modalLogonHeader ',
        ErrorInfoBlock: ' div[data-errorinfo] ',
        TemplateCacheId: ' #templateCache '
    };
    function lockFormLogon() {
        $('<div>').addClass('loader').appendTo(MonsterApp.CssSelectors.ModalLogonId);
    }
    function unlockFormLogon() {
        $('div.loader').remove();
        $(MonsterApp.CssSelectors.ModalLogonId).modal('hide');
    }
    function showLogOnForm(html, title, oldOptions) {
        $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ModalLogonBodyClass).html(html);
        $(MonsterApp.CssSelectors.ModalLogonHeaderId).text(title);
        $.validator.unobtrusive.parse($(MonsterApp.CssSelectors.ModalLogonId));
        $(MonsterApp.CssSelectors.ModalLogonId).modal('show');
        $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ModalLogonBodyClass + 'form').on('submit', function (e) {
            e.preventDefault();
            var form = $(MonsterApp.CssSelectors.ModalLogonId + 'form');
            if(!form.valid()) {
                return;
            }
            var url = form.attr('action');
            var formData = form.serialize();
            $.ajax({
                url: url,
                data: formData,
                dataType: 'json',
                type: 'post',
                success: function (data) {
                    if(typeof data.Authorized != 'undefined') {
                        if(data.Authorized === false) {
                            showLogOnForm(data.LogOnPage, data.LogOnTitle, oldOptions);
                            $('div.loader').remove();
                            return;
                        }
                        if(data.Authorized === true) {
                            unlockFormLogon();
                            Ajax(oldOptions);
                        }
                    }
                },
                error: function (responce) {
                    var message = MonsterApp.Messages.ServerError;
                    if(typeof responce.Error != 'undefined') {
                        message = responce.Error;
                    }
                    $(MonsterApp.CssSelectors.ModalLogonId + MonsterApp.CssSelectors.ErrorInfoBlock).text(message).show();
                    $('div.loader').remove();
                },
                beforeSend: function () {
                    lockFormLogon();
                }
            });
        });
    }
    function Ajax(options) {
        options || (options = {
        });
        var successFunction = options.success;
        options.success = function (data) {
            if(typeof data.Error != 'undefined') {
                if($.isFunction(options.error)) {
                    options.error(data);
                }
                return;
            }
            if(typeof data.Authorized != 'undefined' && data.Authorized === false) {
                showLogOnForm(data.LogOnPage, data.LogOnTitle, options);
                $('div.loader').remove();
                return;
            }
            if($.isFunction(successFunction)) {
                successFunction(data);
            }
        };
        return $.ajax(options);
    }
    MonsterApp.Ajax = Ajax;
})(MonsterApp || (MonsterApp = {}));
