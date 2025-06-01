var GeolocationPlugin = {
    $GeolocationState: {
        latitude: 0,
        longitude: 0,
        hasData: false,
        isAvailable: false,
        watchId: null
    },

    IsGeolocationAvailable: function() {
        GeolocationState.isAvailable = "geolocation" in navigator;
        return GeolocationState.isAvailable;
    },

    RequestGeolocation: function() {
        if (!navigator.geolocation) {
            console.log("Geolocation is not supported by this browser");
            return;
        }

        // Request current position
        navigator.geolocation.getCurrentPosition(
            function(position) {
                console.log("Geolocation success:", position.coords.latitude, position.coords.longitude);
                GeolocationState.latitude = position.coords.latitude;
                GeolocationState.longitude = position.coords.longitude;
                GeolocationState.hasData = true;
                
                // Start watching position for updates
                if (GeolocationState.watchId) {
                    navigator.geolocation.clearWatch(GeolocationState.watchId);
                }
                
                GeolocationState.watchId = navigator.geolocation.watchPosition(
                    function(position) {
                        GeolocationState.latitude = position.coords.latitude;
                        GeolocationState.longitude = position.coords.longitude;
                        GeolocationState.hasData = true;
                    },
                    function(error) {
                        console.log("Geolocation watch error:", error.message);
                    },
                    {
                        enableHighAccuracy: true,
                        timeout: 10000,
                        maximumAge: 30000
                    }
                );
            },
            function(error) {
                console.log("Geolocation error:", error.message);
                GeolocationState.hasData = false;
                
                switch(error.code) {
                    case error.PERMISSION_DENIED:
                        console.log("User denied the request for Geolocation.");
                        break;
                    case error.POSITION_UNAVAILABLE:
                        console.log("Location information is unavailable.");
                        break;
                    case error.TIMEOUT:
                        console.log("The request to get user location timed out.");
                        break;
                    default:
                        console.log("An unknown error occurred.");
                        break;
                }
            },
            {
                enableHighAccuracy: true,
                timeout: 15000,
                maximumAge: 30000
            }
        );
    },

    GetLatitude: function() {
        return GeolocationState.latitude;
    },

    GetLongitude: function() {
        return GeolocationState.longitude;
    },

    HasLocationData: function() {
        return GeolocationState.hasData;
    }
};

autoAddDeps(GeolocationPlugin, '$GeolocationState');
mergeInto(LibraryManager.library, GeolocationPlugin); 