document.addEventListener('DOMContentLoaded', function () {
    function qs(sel, ctx) { return (ctx || document).querySelector(sel); }
    function qsa(sel, ctx) { return Array.from((ctx || document).querySelectorAll(sel)); }

    const panel = qs('#product-details-panel');
    const img = qs('#panel-img');
    const nameEl = qs('#panel-name');
    const categoryEl = qs('#panel-category');
    const priceEl = qs('#panel-price');
    const supplierEl = qs('#panel-supplier');
    const descriptionEl = qs('#panel-description');

    const editForm = qs('#panel-edit-form');
    const viewMode = qs('#panel-view-mode');
    const editToggle = qs('#panel-edit-toggle');
    const closeBtn = qs('#panel-close');
    const editCancel = qs('#edit-cancel');

    function openPanel() { panel.style.display = 'block'; window.scrollTo(0, document.body.scrollHeight); }
    function closePanel() { panel.style.display = 'none'; hideEdit(); }

    qsa('.btn-details').forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.dataset.id;
            if (!id) return;
            fetch(`/Products/DetailsJson/${id}`)
                .then(r => {
                    if (!r.ok) throw new Error('Не удалось загрузить детали');
                    return r.json();
                })
                .then(data => {
                    img.src = `/images/products/${data.Id}.jpg`;
                    img.onerror = function () { this.src = '/images/placeholder.png'; };
                    nameEl.textContent = data.Name || '';
                    categoryEl.textContent = data.Category || '';
                    priceEl.textContent = (data.Price !== undefined) ? new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB' }).format(data.Price) : '';
                    supplierEl.textContent = data.Supplier ? ('Поставщик: ' + data.Supplier) : '';
                    descriptionEl.textContent = data.Description || '';

                    qs('#edit-id').value = data.Id || '';
                    qs('#edit-name').value = data.Name || '';
                    qs('#edit-price').value = data.Price ?? '';
                    qs('#edit-quantity').value = data.Quantity ?? '';
                    qs('#edit-category').value = data.CategoryId ?? '';
                    qs('#edit-supplier').value = data.SupplierId ?? '';
                    qs('#edit-description').value = data.Description || '';

                    openPanel();
                })
                .catch(err => {
                    console.error(err);
                    alert('Ошибка при загрузке деталей.');
                });
        });
    });

    closeBtn.addEventListener('click', closePanel);

    function showEdit() {
        viewMode.style.display = 'none';
        editForm.style.display = 'block';
        editToggle.textContent = 'Просмотр';
    }
    function hideEdit() {
        viewMode.style.display = 'block';
        editForm.style.display = 'none';
        editToggle.textContent = 'Редактировать';
    }

    editToggle.addEventListener('click', function () {
        if (editForm.style.display === 'block') hideEdit(); else showEdit();
    });

    editCancel.addEventListener('click', function () { hideEdit(); });

    editForm.addEventListener('submit', function (e) {
        e.preventDefault();
        const id = qs('#edit-id').value;
        const formData = new FormData(editForm);

        // Add antiforgery token if present on the page
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) formData.append('__RequestVerificationToken', tokenInput.value);

        fetch(`/Products/EditJson/${id}`, {
            method: 'POST',
            body: formData
        })
            .then(r => {
                if (!r.ok) throw new Error('Ошибка сохранения');
                return r.json();
            })
            .then(res => {
                location.reload();
            })
            .catch(err => {
                console.error(err);
                alert('Не удалось сохранить. См. консоль.');
            });
    });
});
