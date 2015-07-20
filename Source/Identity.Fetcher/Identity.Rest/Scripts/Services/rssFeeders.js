angular.module('inspire').factory('rssFeederService', ['$http','$q', function ($http, $q) {

    var o = {

    };

    o.getById = function (id) {

        var deferred = $q.defer();

        $http.get('/Api/RssFeeder/' + id).success(function (data) {
        
            deferred.resolve({ data: data });
        });

        return deferred.promise;
    };

    o.update = function(rssFeeder) {

        $http.put('/Api/RssFeeder/' + rssFeeder.Id, rssFeeder);
    }


    return o;
}]);
