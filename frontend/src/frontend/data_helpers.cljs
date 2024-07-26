(ns frontend.data-helpers)

(defn empty-space->nil [s] (when (not= "" s) s))

(defn normalize-habit [h]
  (update h :description empty-space->nil))

(defn normalize-ct [ct]
  (update ct :description empty-space->nil))
