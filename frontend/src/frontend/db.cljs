(ns frontend.db
  (:require [frontend.localization :as l]))

(def default-db
  {:name "re-frame"
   :panel :login
   :locale :eng
   :locales l/one-big-dictionary
   :login {}
   :alerts []})
