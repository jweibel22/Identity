angular.module('inspire')
    .controller('NavigatorCtrl', [
        '$scope', '$modal', '$state', '$window', '$location', 'authService', 'postService', 'channelService', 'userPromise', 
        function ($scope, $modal, $state, $window, $location, authService, postService, channelService, userPromise) {

            $scope.user = userPromise.data;
            $scope.state = $state;
            $scope.searchFor = "";

            $scope.addLinkWindowdata = {
                user: $scope.user,
                link: '',
                linktags: ''
            }

            $scope.addPostWindowdata = {
                user: $scope.user,
                title: '',
                description: '',
                tags: ''
            }

            var guid = function() {
                function _p8(s) {
                    var p = (Math.random().toString(16)+"000000000").substr(2,8);
                    return s ? "-" + p.substr(0,4) + "-" + p.substr(4,4) : p ;
                }
                return _p8() + _p8(true) + _p8(true) + _p8();
            }

            $scope.logOut = function () {
                authService.logOut();
                //window.location = '#/home';
            }

            $scope.authentication = authService.authentication;

            $scope.addLink = function () {

                $modal.open({
                    templateUrl: 'addLinkModal.html',
                    backdrop: true,
                    windowClass: 'modal',
                    controller: function ($scope, $modalInstance, windowdata) {
                        $scope.windowdata = windowdata;

                        $scope.submit = function () {

                            if(!$scope.windowdata.link || $scope.windowdata.link === '') {
                                $modalInstance.dismiss('cancel');
                                return;
                            }

                            postService.create({
                                Title: null,
                                Description: null,
                                Uri: $scope.windowdata.link,
                                Type: "link",
                                Tags : $scope.windowdata.linktags ? $scope.windowdata.linktags.split(' ') : null,
                                Created : new Date()
                            }, $scope.windowdata.user.SavedChannel);

                            $modalInstance.dismiss('cancel');
                        }
                        $scope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    },
                    resolve: {
                        windowdata: function () {
                            return $scope.addLinkWindowdata;
                        }
                    }
                });
            };


            $scope.addPost = function () {

                $modal.open({
                    templateUrl: 'addPostModal.html',
                    backdrop: true,
                    windowClass: 'modal',
                    controller: function ($scope, $modalInstance, windowdata) {
                        $scope.windowdata = windowdata;

                        $scope.submit = function () {

                            if(!$scope.windowdata.title || $scope.windowdata.title === '') {
                                $modalInstance.dismiss('cancel');
                                return;
                            }

                            postService.create({
                                title: $scope.windowdata.title,
                                description: $scope.windowdata.description,
                                uri: guid(),
                                type: "userpost",
                                tags : $scope.windowdata.tags ? $scope.windowdata.tags.split(' ') : null,
                                created : new Date()
                            }, $scope.windowdata.user.SavedChannel);

                            $modalInstance.dismiss('cancel');
                        }
                        $scope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    },
                    resolve: {
                        windowdata: function () {
                            return $scope.addPostWindowdata;
                        }
                    }
                });
            };

            $scope.search = function(event) {
                var url = "#/search?query="+ $scope.searchFor;
                $scope.searchFor = "";
                $window.location.href = url;
            }
        }
    ]);
