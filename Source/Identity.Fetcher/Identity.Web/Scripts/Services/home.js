angular.module('inspire').factory('homeService', ['$http', '$q', 'ngSettings', function ($http, $q, ngSettings) {
    var o = {

    };

    o.getHomeScreenContents = function () {
        var deferred = $q.defer();

        $http.get(ngSettings.baseUrl + '/Api/Home').success(function (data) {

            deferred.resolve({ data: data });
        });

        return deferred.promise;
    };

    return o;
}]);