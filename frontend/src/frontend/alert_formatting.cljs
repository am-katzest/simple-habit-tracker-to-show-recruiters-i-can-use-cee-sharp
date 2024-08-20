(ns frontend.alert-formatting
  (:require [frontend.localization :refer [tr-error]]
            [frontend.errors :as e]))

(defn error-maker [type]
  (fn [body & _ignored]
    {:body body :alert-type type}))

(def info (error-maker :info))
(def warning (error-maker :warning))
(def danger (error-maker :danger))

(def formatters
  {::e/duplicate-username
   (fn [{:keys [part1 part2]} {:keys [login]}]
     (warning [:div part1 " " [:q login] " " part2]))
   ::e/invalid-username-or-password warning
   ::e/expired-token danger
   ::e/habit-not-found danger
   ::e/completion-type-not-found danger
   ::e/completion-not-found danger
   ::e/unknown-error danger})

(defn format-alert [key data]
  (let [formatter (formatters key)
        translated (tr-error key)]
    (when (and formatter translated)
      (formatter translated data))))
