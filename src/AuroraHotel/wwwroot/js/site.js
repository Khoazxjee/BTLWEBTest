(() => {
  'use strict';
  const money = value => new Intl.NumberFormat('vi-VN').format(value) + ' VND';
  document.querySelector('.menu-toggle')?.addEventListener('click', function () { const open = document.querySelector('#main-nav').classList.toggle('open'); this.setAttribute('aria-expanded', String(open)); });
  document.querySelector('.staff-menu-toggle')?.addEventListener('click', function () { const open = document.querySelector('#staff-nav').classList.toggle('open'); this.setAttribute('aria-expanded', String(open)); });
  document.addEventListener('keydown', e => { if (e.key !== 'Escape') return; for (const [panel, toggle] of [['#staff-nav','.staff-menu-toggle'],['#main-nav','.menu-toggle']]) { const nav = document.querySelector(panel); if (nav?.classList.contains('open')) { nav.classList.remove('open'); const button = document.querySelector(toggle); button?.setAttribute('aria-expanded','false'); button?.focus(); } } });
  document.addEventListener('click', e => { const sidebar = document.querySelector('#staff-nav'); if (sidebar?.classList.contains('open') && !sidebar.contains(e.target) && !e.target.closest('.staff-menu-toggle')) { sidebar.classList.remove('open'); document.querySelector('.staff-menu-toggle')?.setAttribute('aria-expanded', 'false'); } });
  const imageFallback = img => { if (img.dataset.failed) return; img.dataset.failed = '1'; img.src = '/images/aurora.svg'; img.classList.add('image-failed'); };
  document.addEventListener('error', e => { if (e.target instanceof HTMLImageElement) imageFallback(e.target); }, true);
  document.querySelectorAll('img').forEach(img => { if (img.complete && img.naturalWidth === 0) imageFallback(img); });
  document.querySelectorAll('.stay-estimate').forEach(form => {
    const update = () => { const a = form.querySelector('[name="ArrivalDate"]').value; const d = form.querySelector('[name="DepartureDate"]').value; const nights = (Date.parse(d) - Date.parse(a)) / 86400000; const output = form.querySelector('.estimate-total'); output.replaceChildren(); if (!Number.isFinite(nights) || nights <= 0) { output.textContent = 'Chọn ngày trả phòng sau ngày nhận phòng.'; return; } const text = document.createElement('span'); text.textContent = `${nights} đêm × ${money(Number(form.dataset.rate))} · Dự kiến`; const total = document.createElement('strong'); total.textContent = money(nights * Number(form.dataset.rate)); output.append(text, total); };
    form.addEventListener('input', update); update();
  });
  document.querySelectorAll('textarea[data-editor]').forEach(textarea => {
    const toolbar = document.createElement('div'); toolbar.className = 'editor-toolbar'; const editor = document.createElement('div'); editor.className = 'rich-editor'; editor.contentEditable = 'true'; editor.setAttribute('role', 'textbox'); editor.setAttribute('aria-multiline', 'true'); editor.setAttribute('aria-label', 'Mô tả phòng có định dạng'); editor.innerHTML = textarea.value;
    [['Đậm','bold'],['Nghiêng','italic'],['Danh sách','insertUnorderedList'],['Xóa định dạng','removeFormat']].forEach(([label, command]) => { const button = document.createElement('button'); button.type = 'button'; button.textContent = label; button.addEventListener('click', () => { editor.focus(); document.execCommand(command, false); textarea.value = editor.innerHTML; }); toolbar.append(button); });
    editor.addEventListener('input', () => { textarea.value = editor.innerHTML; }); textarea.hidden = true; textarea.before(toolbar, editor);
  });
  document.querySelectorAll('form').forEach(form => form.addEventListener('submit', e => {
    if (form.dataset.confirm && !form.dataset.confirmed) {
      e.preventDefault();
      const dialog = document.createElement('dialog'); dialog.className = 'confirm-dialog'; dialog.setAttribute('aria-labelledby', 'confirm-title');
      const heading = document.createElement('h2'); heading.id = 'confirm-title'; heading.textContent = 'Xác nhận thao tác';
      const message = document.createElement('p'); message.textContent = form.dataset.confirm;
      const actions = document.createElement('div'); actions.className = 'actions';
      const cancel = document.createElement('button'); cancel.type = 'button'; cancel.className = 'btn btn-light'; cancel.textContent = 'Quay lại'; cancel.autofocus = true;
      const accept = document.createElement('button'); accept.type = 'button'; accept.className = 'btn btn-primary'; accept.textContent = 'Xác nhận';
      cancel.addEventListener('click', () => dialog.close()); dialog.addEventListener('close', () => dialog.remove());
      accept.addEventListener('click', () => { form.dataset.confirmed = '1'; dialog.close(); form.requestSubmit(); });
      actions.append(cancel, accept); dialog.append(heading, message, actions); document.body.append(dialog); dialog.showModal(); return;
    }
    if ((form.getAttribute('method') || 'get').toLowerCase() === 'post' && form.checkValidity()) { form.querySelectorAll('button[type="submit"],button:not([type])').forEach(button => { button.disabled = true; button.dataset.originalText = button.textContent; button.textContent = 'Đang xử lý…'; }); }
  }));
  window.addEventListener('pageshow', () => document.querySelectorAll('button[data-original-text]').forEach(button => { button.disabled = false; button.textContent = button.dataset.originalText; }));
  document.querySelectorAll('[data-print]').forEach(button => button.addEventListener('click', () => window.print()));
  const search = document.querySelector('#room-search');
  if (search) {
    const results = document.querySelector('#room-results'); const feedback = document.querySelector('#search-feedback'); let abort;
    const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    const load = async (params, push = true) => {
      abort?.abort(); const controller = new AbortController(); abort = controller; results.classList.add('search-loading'); feedback.textContent = 'Đang tìm phòng phù hợp…';
      for (const [key, value] of [...params.entries()]) if (value === '') params.delete(key);
      try {
        const response = await fetch('/api/rooms/availability?' + params, { signal: controller.signal }); const data = await response.json(); if (!response.ok) throw new Error(data.error || 'Tiêu chí tìm kiếm chưa hợp lệ. Vui lòng kiểm tra lại.');
        if (push) history.pushState(null, '', '/Rooms?' + params);
        const cards = data.items.map(room => { const p = new URLSearchParams({ arrivalDate: params.get('ArrivalDate') || '', departureDate: params.get('DepartureDate') || '', guestCount: params.get('GuestCount') || '2' }); p.set('returnUrl', '/Rooms?' + params); const href = '/Rooms/Details/' + room.id + '?' + p; return `<article class="room-card"><a class="room-image" href="${escape(href)}"><img src="${escape(room.imagePath)}" alt="${escape(room.roomType)}" loading="lazy"><span class="image-label">PHÒNG ${escape(room.roomNumber)}</span></a><div class="room-content"><small>${room.capacity} KHÁCH · KHÔNG GIAN RIÊNG</small><h3>${escape(room.roomType)}</h3><p class="amenities">${escape(room.amenities.split(',').slice(0,3).join(' · '))}</p><div class="room-bottom"><div><strong>${money(room.nightlyRate)}</strong><small>/ đêm</small></div><a class="arrow-link" aria-label="Xem phòng ${escape(room.roomNumber)}" href="${escape(href)}">↗</a></div></div></article>`; }).join('');
        let paging = ''; for (let page = Math.max(1, data.page - 2); page <= Math.min(data.pages, data.page + 2); page++) { const p = new URLSearchParams(params); p.set('page', page); paging += `<a class="page-link ${page === data.page ? 'active' : ''}" href="/Rooms?${escape(p.toString())}" ${page === data.page ? 'aria-current="page"' : ''}>${page}</a>`; }
        results.innerHTML = `<div class="section-heading compact"><p><strong>${data.total} phòng</strong> phù hợp với kỳ nghỉ của bạn</p><small>Kết quả tại thời điểm tìm · Giá mỗi đêm</small></div>` + (data.total ? `<div class="room-grid">${cards}</div>` : '<div class="empty-state"><span>⌕</span><h2>Chưa tìm thấy phòng phù hợp</h2><p>Thử thay đổi ngày ở, số khách hoặc mở rộng khoảng giá.</p></div>') + (data.pages > 1 ? `<nav class="pagination-wrap" aria-label="Phân trang">${paging}<span>Trang ${data.page} / ${data.pages}</span></nav>` : '');
        feedback.textContent = `Đã tìm thấy ${data.total} phòng.`;
      } catch (error) { if (error.name !== 'AbortError') { feedback.textContent = error.message || 'Không thể tải dữ liệu. Vui lòng thử lại.'; results.innerHTML = '<div class="empty-state"><h2>Chưa thể hiển thị kết quả</h2><p>Kiểm tra tiêu chí và bấm Tìm phòng trống để thử lại.</p></div>'; } }
      finally { if (abort === controller) results.classList.remove('search-loading'); }
    };
    search.addEventListener('submit', e => { e.preventDefault(); const params = new URLSearchParams(new FormData(search)); params.set('page', '1'); load(params); });
    results.addEventListener('click', e => { const anchor = e.target.closest('.pagination-wrap a'); if (anchor) { e.preventDefault(); load(new URL(anchor.href).searchParams); } });
    window.addEventListener('popstate', () => { const params = new URLSearchParams(location.search); [...search.elements].forEach(input => { if (input.name) input.value = params.get(input.name) ?? (input.name === 'GuestCount' ? '2' : input.name === 'Sort' ? 'price' : ''); }); load(params, false); });
  }
})();
