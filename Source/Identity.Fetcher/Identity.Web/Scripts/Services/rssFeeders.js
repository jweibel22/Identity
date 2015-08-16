angular.module('inspire').factory('rssFeederService', ['$http', '$q', 'ngSettings', function ($http, $q, ngSettings) {

    var o = {

    };

    o.getById = function (id) {

        var deferred = $q.defer();

        $http.get(ngSettings.baseUrl + '/Api/RssFeeder/' + id).success(function (data) {
        
            deferred.resolve({ data: data });
        });

        return deferred.promise;
    };

    o.update = function(rssFeeder) {

        $http.put(ngSettings.baseUrl + '/Api/RssFeeder/' + rssFeeder.Id, rssFeeder);
    }


    return o;
}]);
