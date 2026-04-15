function init() {
    const path = window.location.pathname.toLowerCase();
    if (path.startsWith('/gallery/login')) {
        import('@pages/gallery/login').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/gallery')) {
        import('@pages/gallery/gallery').then(({ default: init }) => { init(); });
    }
}

export default init;