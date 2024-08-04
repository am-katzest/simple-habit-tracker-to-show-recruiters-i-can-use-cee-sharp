(ns frontend.alert-formatting
  (:require [frontend.localization :refer [tr-error]]))

(defn error-maker [type]
  (fn [body & _ignored]
    {:body body :alert-type type}))

(def info (error-maker :info))
(def warning (error-maker :warning))
(def danger (error-maker :danger))

(def formatters
  {"duplicate username"
   (fn [{:keys [part1 part2]} {:keys [login]}]
     (warning [:div part1 " " [:q login] " " part2]))
   "invalid username or password" warning
   "invalid token" danger
   "habit not found" danger
   "completion type not found" danger})

(defn format-alert [key data]
  (let [formatter (formatters key)
        translated (tr-error key)]
    (if (and formatter translated)
      (formatter translated data)
      (info (str "unknown error: " key)))))
