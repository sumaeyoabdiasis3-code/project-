// Mobile off-canvas sidebar toggle
(function () {
  var toggle = document.getElementById('sidebarToggle');
  var backdrop = document.getElementById('sidebarBackdrop');

  function closeSidebar() {
    document.body.classList.remove('sidebar-open');
  }

  if (toggle) {
    toggle.addEventListener('click', function () {
      document.body.classList.toggle('sidebar-open');
    });
  }

  if (backdrop) {
    backdrop.addEventListener('click', closeSidebar);
  }

  document.querySelectorAll('.sidebar-nav .nav-link').forEach(function (link) {
    link.addEventListener('click', closeSidebar);
  });
})();
