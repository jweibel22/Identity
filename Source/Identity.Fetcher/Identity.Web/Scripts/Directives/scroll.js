angular.module('inspire').directive("scroll", function ($window) {

    function checkVisible(elm) {
        var vpH = $(window).height(), // Viewport Height
        st = $(window).scrollTop(), // Scroll Top
        y = $(elm).offset().top,
        elementHeight = $(elm).height();

        var res = ((y < (vpH + st)) && (st < (y + elementHeight)));
        return res;
    };

    return function (scope, element, attrs) {
        angular.element($window).bind("scroll", function () {

            var divs = $(element).find('div.readable');

            for (var i = 1; i < divs.length; i++) {
                if (checkVisible(divs[i])) {
                    scope.read(scope.posts[i - 1]);
                }
            }

            scope.$apply();
        });
    };
});