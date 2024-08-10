(ns frontend.styles
  (:require-macros
    [garden.def :refer [defcssfn]])
  (:require
    [spade.core   :refer [defglobal defclass]]
    [garden.units :refer [deg px]]
    [garden.color :refer [rgba]]))

(defcssfn linear-gradient
 ([c1 p1 c2 p2]
  [[c1 p1] [c2 p2]])
 ([dir c1 p1 c2 p2]
  [dir [c1 p1] [c2 p2]]))

(defglobal defaults
  [:body
   {:color               :black
    :background-color    :#ddd
    :background-image    [(linear-gradient :white (px 2) :transparent (px 2))
                          (linear-gradient (deg 90) :white (px 2) :transparent (px 2))
                          (linear-gradient (rgba 255 255 255 0.3) (px 1) :transparent (px 1))
                          (linear-gradient (deg 90) (rgba 255 255 255 0.3) (px 1) :transparent (px 1))]
    :background-size     [[(px 100) (px 100)] [(px 100) (px 100)] [(px 20) (px 20)] [(px 20) (px 20)]]
    :background-position [[(px -2) (px -2)] [(px -2) (px -2)] [(px -1) (px -1)] [(px -1) (px -1)]]}])

(defglobal fix-alerts-being-invisible
  [:.rc-alert.fade {:opacity 1}])

(defglobal nav-disabled
  [:.nav-disabled {:color "#ccc"}])

(defglobal fix-rc-datepicker
  [:.rc-datepicker-selected {:color "#ffffff" :background-color "#6c757d"}])

(defglobal calendar-cell-outline
  (let [fraction 90
        rnd 20
        stroke (* 2 (- 100 fraction))]
    [:.calendar-cell-outline
     {:x (px (- fraction))
      :y (px (- fraction))
      :width (px (* 2 fraction))
      :height (px (* 2 fraction))
      :fill :transparent
      :stroke :black
      :rx (px rnd)
      :ry (px rnd)
      :stroke-width (px stroke)}]))

(defclass level1
  []
  {:color :green})

(defclass habit-panel-right [] {:background-color "#eee"})

(defclass habit-subpanel [] {:background-color "#fff"})
