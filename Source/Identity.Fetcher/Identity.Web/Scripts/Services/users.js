angular.module('inspire').factory('userService', ['$http', '$q', 'ngSettings', function ($http, $q, ngSettings) {

    var o = {
        searchResult: [],
        currentUser: null,
        userPromise : null
};

    o.getCurrentUser = function () {

        o.userPromise = $q.defer();

        $http.get(ngSettings.baseUrl + '/Api/User').success(function(data) {
                angular.copy(data, o.currentUser);
                o.userPromise.resolve({ data: data });
            });
        
        return o.userPromise.promise;
    };

    o.getUser = function (userId) {
        return $http.get(ngSettings.baseUrl + '/Api/User/' + userId);
    };

    o.findByName = function (query) {
        
        var result = $q.defer();
        $http.get(ngSettings.baseUrl + '/Api/User?query=' + query).success(function (data) {
            angular.copy(data, o.searchResult);
            result.resolve(data);
        });
        return result.promise;
    }

    o.block = function (userId, tag) {
        return $http.post(ngSettings.baseUrl + '/Api/User/' + userId + '/Block?tag=' + tag);
    }

    o.update = function (user) {
        return $http.post(ngSettings.baseUrl + '/Api/User/' + user.Id + '/Update?isPremium=' + user.IsPremium);
    }

    return o;
}]);
