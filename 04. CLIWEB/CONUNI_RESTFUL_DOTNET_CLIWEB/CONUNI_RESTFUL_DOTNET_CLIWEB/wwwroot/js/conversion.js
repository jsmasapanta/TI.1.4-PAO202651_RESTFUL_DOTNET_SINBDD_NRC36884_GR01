// Actualiza la lista "A" según la unidad elegida en "De" e implementa el botón ⇄.
(function () {
    const origen = document.getElementById('origen');
    const destino = document.getElementById('destino');
    const invertir = document.getElementById('btnInvertir');
    const mapa = window.mapaDestinos || {};

    function cargarDestinos(preferido) {
        const opciones = mapa[origen.value] || [];
        destino.innerHTML = '';
        opciones.forEach(function (u) {
            const opcion = document.createElement('option');
            opcion.value = u.Simbolo;
            opcion.textContent = u.Nombre + ' (' + u.Simbolo + ')';
            destino.appendChild(opcion);
        });
        if (preferido && opciones.some(function (u) { return u.Simbolo === preferido; })) {
            destino.value = preferido;
        }
    }

    origen.addEventListener('change', function () { cargarDestinos(null); });

    invertir.addEventListener('click', function () {
        const nuevoOrigen = destino.value;
        const nuevoDestino = origen.value;
        if (!mapa[nuevoOrigen]) { return; }
        origen.value = nuevoOrigen;
        cargarDestinos(nuevoDestino);
        document.getElementById('valor').focus();
    });
})();
