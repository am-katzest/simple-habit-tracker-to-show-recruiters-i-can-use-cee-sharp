(ns frontend.persistance
  (:require [re-frame.core :as re-frame]))

;; url

(defn sstr->map [sstr]
  (try (->>
        (-> sstr (.substring 1) (.split "&"))
        (map (fn [x] (let [[a b] (.split x "=")]
                      (when (and a b (not= "" a) (not= "" b))
                        [(keyword a) (keyword b)]))))
        (into {}))
       (catch js/Error _ {})))

(defn map->sstr [m]
  (if-not (pos? (count m)) ""
          (->> m
               (map (fn [[a b]] (str (name a) "=" (name b))))
               (reduce #(str %1 "&" %2))
               (str "?"))))

(defn get-url-info []
  (sstr->map (.. js/window -location -search)))

(defn change-url-query "without reloading" [q]
  (let [hist (.. js/window -history)
        url (new js/URL (.. js/window -location -href))]
    (set!  (.-search url) q)
    (.pushState hist nil nil url)))

(defn store-url-info! [m]
  (change-url-query (map->sstr m)))

(defn set-panel! [panel]
  (store-url-info! {:panel panel}))

(defn set-panel-if-none-found! [default]
  (when (= {} (get-url-info))
    (set-panel! default)))

(defn get-panel! []
  (:panel (get-url-info)))

(re-frame/reg-fx
 :set-panel
 (fn [value]
   (set-panel! value)))

;; local storage

(defn get-token []
  (let [t (.getItem (.-localStorage js/window) "token")]
    (when (and (some? t) (not= "" t)) t)))

(defn set-token! [token]
  (.setItem (.-localStorage js/window) "token" (str token)))

(re-frame/reg-fx
 :store-token
 (fn [value]
   (set-token! value)))
