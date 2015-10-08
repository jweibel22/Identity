angular.module('inspire')
    .controller('PostsCtrl', [
        '$scope',
        '$stateParams',
        '$location',
        'postService',
        'post',
        function($scope, $stateParams, $location, postService, post){

            $scope.post = post;
            $scope.title = post.Title;
            $scope.description = post.Description;
            $scope.tags = post.Tags;

            $scope.editPost = function() {
                if($scope.Title === '') { return; }

                postService.editPost({Id: post.Id, Title: $scope.title, Description: $scope.description, Tags: $scope.tags})
                    .success(function(post) {
                        
                    });
            }

            $scope.deletePost = function() {
                postService.deletePost($scope.post)
                    .success(function(post) {
                        $scope.post.Title = "";
                        $scope.post.Description = "";
                    });
            }

            $scope.addComment = function(){
                if($scope.body === '') { return; }
                postService.addComment(post.Id, {
                    body: $scope.body,
                    author: 'user'
                }).success(function(comment) {
                    $scope.post.Comments.push(comment);
                });
                $scope.body = '';
            };

            $scope.incrementUpvotes = function(comment){
                postService.upvoteComment(post, comment);
            };

        }]);
