(ns frontend.svg
  (:require [clojure.math :as math]))

(defn name-angle [begin end]
  (str "cutout-angle" begin "-" end))

(defn circle-coords [x scale]
  (str (* (math/sin x) scale) " "
       (* (math/cos x) scale) " "))

(defn linint [a b x]
  (+ (* b x) (* a (- 1 x))))

(defn create-clip-path [{:keys [begin end]}]
  ^{:key begin}
  [:clipPath {:id (name-angle begin end)}
   [:path
    {:d
     (str "M" (circle-coords 0 0)
          "L" (circle-coords begin 1000)
          "L" (circle-coords (linint begin end 0.2) 1000)
          "L" (circle-coords (linint begin end 0.4) 1000)
          "L" (circle-coords (linint begin end 0.6) 1000)
          "L" (circle-coords (linint begin end 0.8) 1000)
          "L" (circle-coords end 1000)
          "Z")}]])

(defn output-calendar-outline-svg [geoms]
  [:svg {:width "100%" :height "100%" :viewBox "-100 -100 200 200" :xmlns "http://www.w3.org/2000/svg"}
   [:defs
    [:<> (map create-clip-path geoms)]]
   [:<> (map (fn [{:keys [color begin end]}]
               ^{:key begin}
               [:rect.calendar-cell-outline {:style {:stroke color} :clip-path (str "url(#" (name-angle begin end) ")")}])
             geoms)]])

(defn output-calendar-outline-svg-single [color]
  [:svg {:width "100%" :height "100%" :viewBox "-100 -100 200 200" :xmlns "http://www.w3.org/2000/svg"}
   [:rect.calendar-cell-outline {:style {:stroke color}}]])

(def gap-width 0.1)

(defn process-geoms [colors]
  (let [lines (count colors)
        total-width (+ lines (* gap-width lines))
        scale #(/ (* % 6.28318530718) total-width)]
    (loop [acc []
           [color & rest] colors
           so-far (/ gap-width 2)]
      (if-not color acc
              (recur (conj acc {:color color
                                :begin (scale so-far)
                                :end (scale (+ so-far 1))})
                     rest
                     (+ so-far 1 gap-width))))))

(defn make-calendar-outline [colors]
  (cond
    (= 0 (count colors)) [:div]
    (= 1 (count colors)) (output-calendar-outline-svg-single (first colors))
    :else (output-calendar-outline-svg (process-geoms colors))))
