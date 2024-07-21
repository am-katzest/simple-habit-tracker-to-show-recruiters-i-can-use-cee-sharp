(ns frontend-test.helpers
  (:require [frontend-test.setup :as s :refer-macros true]
            [etaoin.api :as e]
            [clojure.test :refer [is]]))

(defn tag-selector-factory-factory [x]
  (fn self
    ([] {:tag x})
    ([id] (assoc (self) :data-testid id))
    ([id enabled] (assoc (self id) :fn/enabled enabled))))

(def btn (tag-selector-factory-factory :button))

(def input (tag-selector-factory-factory :input))

(defn wrap-waiter-into-test [f]
  (fn [& args]
    (is (any? (apply f s/*driver* args)) args)))

(def wait-enabled (wrap-waiter-into-test e/wait-enabled))
(def wait-exists (wrap-waiter-into-test e/wait-exists))
(def wait-disabled (wrap-waiter-into-test e/wait-disabled))
(def wait-predicate e/wait-predicate)   ; one of the functions ignoring convention

(defn query [s]
  (reduce str (drop-while (complement #{\?}) s)))

(def short-wait
  {:timeout 0.5
   :interval 0.01})
(defmacro with-wait [params & exprs]
  `(binding [e/*wait-timeout* (:timeout ~params)
             e/*wait-interval* (:interval ~params)]
     ~@exprs))
