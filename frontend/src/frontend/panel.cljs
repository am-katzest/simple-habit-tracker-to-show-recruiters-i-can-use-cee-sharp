(ns frontend.panel)

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

(defn store-url-info! [m]
  (set! (.. js/window -location -search) (map->sstr m)))

(defn initialize-to-login-if-no-found! []
  (let [current (get-url-info)]
    (when-not current
      (store-url-info! {:panel :login}))))

(defn get-panel! []
  (initialize-to-login-if-no-found!)
  (:panel (get-url-info)))

(defn set-panel! [panel]
  (store-url-info! {:panel panel}))
