console.log("[DEBUG] Início do carregamento de mapsInterop.js");

window.mapsInterop = window.mapsInterop || {};

console.log("[DEBUG] window.mapsInterop definido:", window.mapsInterop);

// Função para carregar a API do Google Maps e o MarkerClusterer
window.mapsInterop.loadGoogleMaps = function (apiKey) {
    console.log("[DEBUG] loadGoogleMaps chamado com API Key:", apiKey);

    // Carrega o script do Google Maps
    if (!document.querySelector('script[src*="maps.googleapis.com"]')) {
        const googleMapsScript = document.createElement('script');
        googleMapsScript.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&callback=initMap&v=quarterly&libraries=places&loading=async`;
        googleMapsScript.async = true;
        googleMapsScript.defer = true;
        googleMapsScript.onerror = function () {
            console.error("[ERROR] Falha ao carregar o script do Google Maps.");
        };
        googleMapsScript.onload = function () {
            console.log("[DEBUG] Script do Google Maps carregado com sucesso.");
        };
        document.head.appendChild(googleMapsScript);
    } else {
        console.log("[DEBUG] Script do Google Maps já está no DOM.");
    }

    // Carrega o script do MarkerClusterer
    if (!document.querySelector('script[src*="markerclusterer"]')) {
        const clusterScript = document.createElement('script');
        clusterScript.src = "https://unpkg.com/@googlemaps/markerclusterer/dist/index.min.js";
        clusterScript.async = true;
        clusterScript.defer = true;
        clusterScript.onerror = function () {
            console.error("[ERROR] Falha ao carregar o script do MarkerClusterer.");
        };
        clusterScript.onload = function () {
            console.log("[DEBUG] Script do MarkerClusterer carregado com sucesso.");
        };
        document.head.appendChild(clusterScript);
    } else {
        console.log("[DEBUG] Script do MarkerClusterer já está no DOM.");
    }
};

// Inicializa o Google Maps quando o script é carregado
window.initMap = function () {
    window.googleMapsLoaded = true;
    console.log("[DEBUG] Google Maps API carregada com sucesso via initMap!");
};

window.mapsInterop.getUserLocation = async function () {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    console.log("[DEBUG] Geolocalização obtida com sucesso:", position.coords.latitude, position.coords.longitude);
                    resolve({
                        lat: position.coords.latitude,
                        lng: position.coords.longitude
                    });
                },
                (error) => {
                    console.error("[ERROR] Erro ao obter geolocalização:", error.message);
                    reject(new Error("Erro ao obter geolocalização: " + error.message));
                },
                { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
            );
        } else {
            console.error("[ERROR] Geolocalização não é suportada pelo navegador.");
            reject(new Error("Geolocalização não é suportada pelo navegador."));
        }
    });
};

window.mapsInterop.findNearbySupermarkets = async function (lat, lng, elementId, dotNetRef) {
    if (!window.google || !window.google.maps) {
        console.error("[ERROR] Google Maps API não carregada ou chave inválida.");
        return;
    }

    console.log("[DEBUG] Buscando supermercados em lat:", lat, "lng:", lng);
    var location = new google.maps.LatLng(lat, lng);
    var mapElement = document.getElementById(elementId);
    if (!mapElement) {
        console.error("[ERROR] Elemento do mapa não encontrado:", elementId);
        return;
    }

    // Estilo personalizado para remover POIs desnecessários
    var mapStyles = [
        {
            featureType: "poi",
            elementType: "all",
            stylers: [{ visibility: "off" }]
        },
        {
            featureType: "transit",
            elementType: "all",
            stylers: [{ visibility: "off" }]
        }
    ];

    var map = new google.maps.Map(mapElement, {
        zoom: 12,
        center: location,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
        styles: mapStyles // Aplica o estilo personalizado
    });

    var service = new google.maps.places.PlacesService(map);

    // Expande a busca para incluir mais tipos de lojas
    var request = {
        location: location,
        radius: 15000, // 15 km
        type: "store", // Tipo genérico para lojas
        keyword: "supermercado OR mercearia OR minimercado OR mercado OR loja de conveniência OR alimentos OR groceries" // Expande as palavras-chave
    };

    service.nearbySearch(request, function (results, status) {
        if (status === google.maps.places.PlacesServiceStatus.OK) {
            var supermarkets = results
                .filter(place => place.types.includes("store") && !place.types.includes("gas_station")) // Filtra apenas lojas e exclui postos de gasolina
                .map(place => ({
                    name: place.name,
                    placeId: place.place_id,
                    address: place.vicinity,
                    lat: place.geometry.location.lat(),
                    lng: place.geometry.location.lng()
                }));
            console.log("[DEBUG] Supermercados encontrados:", supermarkets);

            // Limpa marcadores anteriores
            if (window.currentMarkers) {
                window.currentMarkers.forEach(marker => marker.setMap(null));
            }
            window.currentMarkers = [];

            // Cria os marcadores com ícone de carrinho de compras com bola azul
            var markers = supermarkets.map(supermarket => {
                return new google.maps.Marker({
                    position: { lat: supermarket.lat, lng: supermarket.lng },
                    map: map, // Adiciona os marcadores diretamente ao mapa
                    title: supermarket.name,
                    icon: {
                        url: "data:image/svg+xml;charset=UTF-8," + encodeURIComponent(
                            '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="36" viewBox="0 0 24 36">' +
                            '<path fill="#1a73e8" fill-opacity="0.9" d="M12 0C5.373 0 0 5.373 0 12c0 2.765.94 5.313 2.53 7.367l8.53 15.98c.15.28.44.48.81.48s.66-.2.81-.48l8.53-15.98C23.06 17.313 24 14.765 24 12c0-6.627-5.373-12-12-12z"/>' +
                            '<path fill="#ffffff" d="M16 16h-2v-2h-4v2H8l-1 5h10zM9 16.5a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm6 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/>' +
                            '</svg>'
                        ),
                        scaledSize: new google.maps.Size(24, 36),
                        anchor: new google.maps.Point(12, 36)
                    }
                });
            });

            // Adiciona clustering para evitar sobreposição, se disponível
            if (typeof MarkerClusterer !== "undefined") {
                console.log("[DEBUG] MarkerClusterer disponível, aplicando clustering.");
                var markerCluster = new MarkerClusterer({
                    map: map,
                    markers: markers,
                    renderer: {
                        render: ({ count, position }) => {
                            return new google.maps.Marker({
                                label: { text: String(count), color: "white", fontSize: "12px" },
                                position: position,
                                icon: {
                                    url: "http://maps.google.com/mapfiles/ms/icons/yellow-dot.png",
                                    scaledSize: new google.maps.Size(40, 40)
                                },
                                zIndex: 1000
                            });
                        }
                    }
                });
            } else {
                console.warn("[WARN] MarkerClusterer não disponível, marcadores exibidos sem clustering.");
                markers.forEach(marker => marker.setMap(map)); // Garante que os marcadores sejam exibidos
            }

            // Adiciona infowindows aos marcadores
            markers.forEach((marker, index) => {
                var supermarket = supermarkets[index];
                var infowindow = new google.maps.InfoWindow({
                    content: `
                        <div style="font-family: Arial, sans-serif;">
                            <h5 style="margin: 0 0 5px 0; font-size: 16px;">${supermarket.name}</h5>
                            <p style="margin: 0 0 10px 0; font-size: 14px; color: #555;">${supermarket.address}</p>
                            <button onclick="window.selectSupermarket('${supermarket.placeId}')" style="background-color: #1a73e8; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-size: 14px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); transition: background-color 0.3s;">Selecionar</button>
                        </div>
                    `
                });

                marker.addListener("click", () => {
                    if (window.currentInfoWindow) {
                        window.currentInfoWindow.close();
                    }
                    infowindow.open(map, marker);
                    window.currentInfoWindow = infowindow;
                });

                window.currentMarkers.push(marker);
            });

            window.selectSupermarket = function (placeId) {
                var supermarket = supermarkets.find(s => s.placeId === placeId);
                if (supermarket) {
                    dotNetRef.invokeMethodAsync("SelectSupermarket", supermarket);
                }
            };

            // Ajusta o zoom para mostrar todos os marcadores
            if (supermarkets.length > 0) {
                var bounds = new google.maps.LatLngBounds();
                supermarkets.forEach(supermarket => {
                    bounds.extend(new google.maps.LatLng(supermarket.lat, supermarket.lng));
                });
                map.fitBounds(bounds);
            }
        } else {
            console.error("[ERROR] Falha ao buscar supermercados:", status);
        }
    });
};

window.mapsInterop.focusOnSupermarket = function (elementId, lat, lng) {
    var mapElement = document.getElementById(elementId);
    if (!mapElement || !window.google || !window.google.maps) {
        console.error("[ERROR] Mapa ou Google Maps API não encontrados para focar no supermercado.");
        return;
    }
    var map = mapElement.__googleMap;
    if (!map) {
        console.error("[ERROR] Mapa não inicializado para o elemento:", elementId);
        return;
    }
    map.setCenter({ lat: lat, lng: lng });
    map.setZoom(15);
};

window.mapsInterop.initMapaLojaClick = function (lat, lng, elementId, dotNetRef) {
    if (!window.google || !window.google.maps) {
        console.error("[ERROR] Google Maps API não carregada ou chave inválida.");
        return;
    }
    console.log("[DEBUG] Inicializando mapa com lat:", lat, "lng:", lng, "elementId:", elementId);
    var center = { lat: parseFloat(lat), lng: parseFloat(lng) };
    var mapElement = document.getElementById(elementId);
    if (!mapElement) {
        console.error("[ERROR] Elemento do mapa não encontrado:", elementId);
        return;
    }

    // Estilo personalizado para remover POIs desnecessários
    var mapStyles = [
        {
            featureType: "poi",
            elementType: "all",
            stylers: [{ visibility: "off" }]
        },
        {
            featureType: "transit",
            elementType: "all",
            stylers: [{ visibility: "off" }]
        }
    ];

    var map = new google.maps.Map(mapElement, {
        zoom: 15,
        center: center,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
        styles: mapStyles // Aplica o estilo personalizado
    });
    mapElement.__googleMap = map;

    var userMarker = new google.maps.Marker({
        position: center,
        map: map,
        draggable: true
    });

    var geocoder = new google.maps.Geocoder();
    function handleMapClick(e) {
        var latClicked = e.latLng.lat();
        var lngClicked = e.latLng.lng();
        userMarker.setPosition(e.latLng);
        console.log("[DEBUG] Mapa clicado. Novas coordenadas:", latClicked, lngClicked);
        geocoder.geocode({ location: { lat: latClicked, lng: lngClicked } }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK && results[0]) {
                var endereco = results[0].formatted_address;
                console.log("[DEBUG] Endereço obtido:", endereco);
                dotNetRef.invokeMethodAsync("SetCoordinatesWithAddress", latClicked, lngClicked, endereco);
            } else {
                console.error("[ERROR] Falha na geocodificação:", status);
                dotNetRef.invokeMethodAsync("SetCoordinatesWithAddress", latClicked, lngClicked, "Endereço não encontrado");
            }
        });
    }

    map.addListener("click", handleMapClick);
    userMarker.addListener("dragend", function () {
        handleMapClick({ latLng: userMarker.getPosition() });
    });
};

console.log("[DEBUG] Fim do carregamento de mapsInterop.js");
