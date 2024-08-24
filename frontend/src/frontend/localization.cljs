(ns frontend.localization
  (:require
   [frontend.errors :as e]
   [clojure.string :as str]
   [re-frame.core :as re-frame]))

(defn get-translation [path]
  (let [map @(re-frame/subscribe [:locale/map])
        result (get-in map path)]
    (if (and result (string? result)) result
        (str "translate(" path ")"))))

(defn split-kw [kw]
  (if-let [ns (namespace kw)]
    (conj (mapv keyword (str/split ns ".")) (keyword (name kw)))
    [kw]))

(defn tr [kw]
  (get-translation (split-kw kw)))

(defn add-prefix
  ([kw]
   (add-prefix (fn [] []) kw))
  ([existing kws]
   (let [path (concat (existing) (if (keyword? kws) (split-kw kws) kws))]
     (fn ([] path)
       ([last]
        (get-translation (concat path (split-kw last))))))))

(defn tr-error [str]
  (get-in @(re-frame/subscribe [:locale/map]) [:error str]))

(def one-big-dictionary
  {:eng
   (merge #:login
           {:create-new-account "create new account"
            :login "login"
            :go-back "return"
            :password "password"
            :repeat "repeat password"
            :username "username"
            :register-new-account "new account"
            :login-old-account "login"}
          #:nav
           {:logout "logout"
            :account "account"
            :habits "habits"}
           #:habit
           {:add-new "add new"
            :description "add description.."
            :confirm-deletion "really delete this habit?"
            :tabs-select "select view"
            :tab-cts "categories"
            :tab-alerts "alerts"
            :tab-completions "history"
            :new-habit "new habit"
            :name "add name.."}
          #:completion
           {:add-new "new completion"
            :new-completion "create new completion"
            :edit-completion "edit completion"
            :specify-hour "exact time?"
            :type "category"
            :note "note"
            :color "color"
            :use-color "override color?"
            :no-completion-types "no completion types"
            :date-pick "pick"
            :date-now "now"
            :date-unchanged "without change"
            :datepicker-cancel "cancel"
            :datepicker-confirm "accept"
            :add-new-confirm "confirm"
            :edit-confirm "save"
            :cancel "cancel"
            :date-today "today"
            :date-yesterday "yesterday"
            :confirm "confirm"}
          #:ct
           {:add-new "add new"
            :delete-popup-title "specify how to handle completions with this category"
            :delete-popup-confirm "delete category"
            :delete-popup-cancel "cancel"
            :delete-popup-handle "how to modify affected completions"
            :delete-popup-delete "delete completions with this category"
            :delete-popup-color "how to modify completions colors"
            :delete-popup-color-leave "don't change completion colors"
            :delete-popup-color-conditional "change only when completion has no color set"
            :delete-popup-color-always "change completion color even if it already has one"
            :delete-popup-note "leave/append note?"
            :new-ct "category"}
           #:prompt{:confirm "confirm"
                    :cancel "cancel"}
          {:error {::e/duplicate-username {:part1 "username" :part2 "taken, pick another"}
                   ::e/invalid-username-or-password "invalid username or password"
                   ::e/expired-token "token expired, please reload"
                   ::e/habit-not-found "habit not found, deleted?"
                   ::e/completion-type-not-found "category not found, deleted?"
                   ::e/completion-not-found "completion not found, deleted?"
                   ::e/unknown-error "unknown error occured :("}})})
