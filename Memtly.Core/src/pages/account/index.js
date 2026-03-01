function init() {
    const path = window.location.pathname.toLowerCase();
    if (path == '/account') {
        import('@pages/account/admin-panel').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/account/login')) {
        import('@pages/account/login').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/account/register')) {
        import('@pages/account/registration').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/account/forgotpassword')) {
        import('@pages/account/forgot-password').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/account/resetpassword')) {
        import('@pages/account/password-reset').then(({ default: init }) => { init(); });
    }
}

export default init;