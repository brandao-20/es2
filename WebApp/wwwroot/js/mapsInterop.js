window.mapsInterop = {
    initMapaLoja: function(lat, lng, elementId) {
        if (!window.google || !google.maps) {
            console.error("Google Maps API não carregada.");
            return;
        }
        var center = { lat: parseFloat(lat), lng: parseFloat(lng) };
        var map = new google.maps.Map(document.getElementById(elementId), {
            zoom: 15,
            center: center
        });
        new google.maps.Marker({ position: center, map: map });
    }
};
