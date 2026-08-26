(function () {
  var theme = localStorage.getItem('mymusic-theme');
  if (theme === 'light' || theme === 'dark') {
    document.documentElement.setAttribute('data-theme', theme);
  }
})();
