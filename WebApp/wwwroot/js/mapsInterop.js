window.mapsInterop = {
    initMapaLojaClick: function(lat, lng, elementId, dotNetRef) {
        if (!window.google || !google.maps) {
            console.error("Google Maps API não carregada ou key inválida.");
            return;
        }

        var center = { lat: parseFloat(lat), lng: parseFloat(lng) };
        var map = new google.maps.Map(document.getElementById(elementId), {
            zoom: 15,
            center: center
        });

        // Cria um marcador arrastável
        var marker = new google.maps.Marker({
            position: center,
            map: map,
            draggable: true
        });

        // Ao clicar no mapa, move o marker e chama o .NET
        map.addListener("click", function(e) {
            var latClicked = e.latLng.lat();
            var lngClicked = e.latLng.lng();
            marker.setPosition(e.latLng);

            // Invoca método C# para atualizar lat/long
            dotNetRef.invokeMethodAsync("SetCoordinates", latClicked, lngClicked);
        });
    }
};
