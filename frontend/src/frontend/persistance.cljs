(ns frontend.persistance
  (:require [re-frame.core :as re-frame]))

;; url

(defn sstr->map [sstr]
  (try (->>
        (-> sstr (.substring 1) (.split "&"))
        (map (fn [x] (let [[a b] (.split x "=")]
                       [(keyword a) (keyword b)])))
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

(defn initialize-to-login-if-no-found! [default]
  (let [current (get-url-info)]
    (when-not current
      (store-url-info! {:panel default}))))

(defn get-panel! []
  (:panel (get-url-info)))

(defn set-panel! [panel]
  (store-url-info! {:panel panel}))

(re-frame/reg-fx
 :set-panel
 (fn [value]
   (set-panel! value)))

;; local storage

(defn get-token []
  (.getItem (.-localStorage js/window) "token"))

(defn set-token! [token]
  (.setItem (.-localStorage js/window) "token" (str token)))

(re-frame/reg-fx
 :store-token
 (fn [value]
   (set-token! value)))
