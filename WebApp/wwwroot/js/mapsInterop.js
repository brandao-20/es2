console.log("[DEBUG] Início do carregamento de mapsInterop.js");

// Define o objeto mapsInterop
window.mapsInterop = window.mapsInterop || {};

console.log("[DEBUG] window.mapsInterop definido:", window.mapsInterop);

// Define a função loadGoogleMaps
window.mapsInterop.loadGoogleMaps = function (apiKey) {
    console.log("[DEBUG] loadGoogleMaps chamado com API Key:", apiKey);
    if (!document.querySelector('script[src*="maps.googleapis.com"]')) {
        const script = document.createElement('script');
        script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&callback=initMap&v=quarterly&libraries=places`;
        script.async = true;
        script.defer = true;
        script.onerror = function () {
            console.error("[ERROR] Falha ao carregar o script do Google Maps.");
        };
        script.onload = function () {
            console.log("[DEBUG] Script do Google Maps carregado com sucesso.");
        };
        document.head.appendChild(script);
    } else {
        console.log("[DEBUG] Script do Google Maps já está no DOM.");
    }
};

// Verifica se a função foi definida
console.log("[DEBUG] mapsInterop.loadGoogleMaps definido:", typeof window.mapsInterop.loadGoogleMaps);

window.mapsInterop.initMapaLojaClick = function (lat, lng, elementId, dotNetRef) {
    if (!window.google || !window.google.maps) {
        console.error("[ERROR] Google Maps API não carregada ou chave inválida.");
        return;
    }

    console.log("[DEBUG] Inicializando mapa com lat:", lat, "lng:", lng, "elementId:", elementId);
    var center = { lat: parseFloat(lat), lng: parseFloat(lng) };
    var map = new google.maps.Map(document.getElementById(elementId), {
        zoom: 15,
        center: center
    });

    var marker = new google.maps.Marker({
        position: center,
        map: map,
        draggable: true
    });

    map.addListener("click", function (e) {
        var latClicked = e.latLng.lat();
        var lngClicked = e.latLng.lng();
        marker.setPosition(e.latLng);
        dotNetRef.invokeMethodAsync("SetCoordinates", latClicked, lngClicked);
        console.log("[DEBUG] Mapa clicado. Novas coordenadas:", latClicked, lngClicked);
    });
};

// Callback global chamado quando a API carrega
window.initMap = function () {
    window.googleMapsLoaded = true;
    console.log("[DEBUG] Google Maps API carregada com sucesso via initMap!");
};

console.log("[DEBUG] Fim do carregamento de mapsInterop.js");
